using FirebaseAdmin.Auth;
using FirebaseAdmin;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using study4_be.Helper;
using study4_be.Models;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.services.Request;
using study4_be.Services.Request;
using study4_be.Validation;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using study4_be.Services;
[Route("api/[controller]")]
[ApiController]
public class Momo_PaymentController : ControllerBase
{
    private readonly ILogger<Momo_PaymentController> _logger;
    private readonly MomoConfig _momoConfig;
    private readonly HashHelper _hashHelper;
    private readonly FireBaseServices _fireBaseServices;
    private FirebaseApp _firebaseApp;
    private SMTPServices _smtpServices = new SMTPServices();
    private STUDY4Context _context = new STUDY4Context();
    public Momo_PaymentController(ILogger<Momo_PaymentController> logger, IOptions<MomoConfig> momoPaymentSettings, FireBaseServices fireBaseServices)
    {
        _logger = logger;
        _momoConfig = momoPaymentSettings.Value;
        _hashHelper = new HashHelper();
        fireBaseServices = _fireBaseServices;
    }
    [HttpPost]
    public async Task<IActionResult> MakePayment([FromBody] MomoPaymentRequest request)
    {
        try
        {
            var signature = _hashHelper.GenerateSignature(request, _momoConfig);
            var response = await SendPaymentRequest(request, signature);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return Ok(responseContent);
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                return BadRequest($"Yêu cầu thanh toán không thành công. Mã lỗi: {response.StatusCode}. Chi tiết lỗi: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            // Ghi log lỗi
            _logger.LogError(ex, "Lỗi khi thực hiện yêu cầu thanh toán MoMo");

            // Trả về lỗi 500 Internal Server Error
            return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu thanh toán");
        }
    }
    private async Task<HttpResponseMessage> SendPaymentRequest(MomoPaymentRequest request, string signature)
    {
        // Dữ liệu yêu cầu thanh toán
        var paymentData = new
        {
            partnerCode = _momoConfig.PartnerCode,
            storeName = _momoConfig.StoreName,
            storeId = _momoConfig.StoreId,
            subPartnerCode = request.SubPartnerCode,
            requestId = request.RequestId,
            amount = request.Amount,
            orderId = request.OrderId,
            orderInfo = request.OrderInfo,
            redirectUrl = request.RedirectUrl,
            ipnUrl = request.IpnUrl,
            requestType = request.RequestType,
            extraData = request.ExtraData,
            lang = request.Lang,
            signature = signature
        };

        using (var client = new HttpClient())
        {
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(paymentData), Encoding.UTF8, "application/json");
            return await client.PostAsync(_momoConfig.PaymentUrl, content);
        }
    }

    [HttpPost("GetIpnFromMomo")]
    public async Task<IActionResult> GetIpnFromMomo([FromBody] GetMomoIPNResult resultIpn)
    {
        int resultCode = resultIpn.resultCode;
        string orderString = resultIpn.orderId;
                if (resultCode == 0 || resultCode== 9000)
                {
                    if (orderString != null)
                    {
                        return await Buy_Success(orderString);
                    }
                    else
                    {
                        return BadRequest("Order id not exist");
                    }
                }
                else
                {
                    return BadRequest("Have error while update state order, please contact to admin to resolve it");
                }
    }
    [HttpPost("RequestTracking")]
    public async Task<IActionResult> CheckTransactionStatus([FromBody] RequestTrackingStatusMomo req) // Assuming orderId is received as a parameter
    {
        if (string.IsNullOrEmpty(req.orderId))
        {
            return BadRequest("Missing mandatory field: orderId"); // Handle missing orderId
        }

        RequestTrackingStatusMomo trackingQuery = new RequestTrackingStatusMomo()
        {
            partnerCode = _momoConfig.PartnerCode,
            requestId = req.requestId, // Generate a unique requestId
            orderId = req.orderId,
            lang = "vi",
        };

        var signature = _hashHelper.GenerateSignatureToCheckingStatus(trackingQuery, _momoConfig);

        var dataRequest = new
        {
            partnerCode = _momoConfig.PartnerCode,
            requestId = trackingQuery.requestId,
            orderId = trackingQuery.orderId,
            lang = trackingQuery.lang,
            signature = signature
        };
        string aa = "https://payment.momo.vn/v2/gateway/api/query";
        using (var client = new HttpClient())
        {
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(dataRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(aa, content);

            if (response.IsSuccessStatusCode)
            {
                return await Buy_Success(req.orderId);
            }
            else
            {
                // Handle unsuccessful response with error message from MoMo API
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
    }
    public async Task<IActionResult> Buy_Success(string orderId)
    {
        var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        try
        {
            if (existingOrder != null &&  existingOrder.State==false)
            {
                existingOrder.State = true;
                var queryNewUserCourses = new UserCourse
                {
                    UserId = existingOrder.UserId,
                    CourseId = (int)existingOrder.CourseId,
                    Date = DateTime.Now,
                };
                await _context.UserCourses.AddAsync(queryNewUserCourses);
                await _context.SaveChangesAsync();
                return Ok(new { status = 200, order = existingOrder, message = "Update Order State Successful" });
            }
            else if (existingOrder != null && existingOrder.State==true)
            {
                return BadRequest("You Had Bought Before");
            }
            else
            {
                return BadRequest("Order not found");
            }
        }
        catch (Exception e)
        {
            return BadRequest("Has error when Update State of Order" + e);
        }
    }
    [HttpPost("sendEmail")]
    public async Task<IActionResult> SendEmailAsync(string userEmail)
    {
        try
        {
            // Initialize Firebase Auth
            var auth = FirebaseAuth.GetAuth(_firebaseApp);

            // Generate email verification link for the user
            var emailLink = await auth.GenerateEmailVerificationLinkAsync(userEmail);

            // Send the email using Firebase (example: using SendGrid)
            await _smtpServices.SendEmailUsingSendGrid(userEmail, emailLink);

            return Ok("Email sent successfully");
        }
        catch (FirebaseAuthException ex)
        {
            // Handle Firebase Auth exceptions
            return StatusCode(500, $"Failed to send email: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle other exceptions
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

}
