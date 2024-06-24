using Microsoft.AspNetCore.Mvc;
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
using Microsoft.EntityFrameworkCore;
using study4_be.Services.Response;
[Route("api/[controller]")]
[ApiController]
public class Momo_PaymentController : ControllerBase
{
    private readonly ILogger<Momo_PaymentController> _logger;
    private readonly MomoConfig _momoConfig;
    private readonly HashHelper _hashHelper;
    private readonly FireBaseServices _fireBaseServices;
    private SMTPServices _smtpServices;
    private STUDY4Context _context = new STUDY4Context();
    public Momo_PaymentController(ILogger<Momo_PaymentController> logger, IOptions<MomoConfig> momoPaymentSettings, FireBaseServices fireBaseServices,SMTPServices sMTPServices)
    {
        _logger = logger;
        _momoConfig = momoPaymentSettings.Value;
        _hashHelper = new HashHelper();
        _fireBaseServices= fireBaseServices;
        _smtpServices = sMTPServices;
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
            var responseContent = await response.Content.ReadAsStringAsync();
            var existOrder = await _context.Orders.Where(o => o.OrderId == req.orderId).SingleOrDefaultAsync();
            if (response.IsSuccessStatusCode)
            {
                var responseData = Newtonsoft.Json.JsonConvert.DeserializeObject<TrackingMomoResponse>(responseContent);

                // Assuming TrackingResponse has a property named resultCode to capture the MoMo API response code
                if (responseData.resultCode == 0)
                {
                    return await Buy_Success(req.orderId);
                }
                else
                {
                    return HandleMoMoErrorResponse(responseData.resultCode);
                }
            }
            else
            {
                // Handle unsuccessful response with error message from MoMo API
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
        }
    }
    private IActionResult HandleMoMoErrorResponse(int resultCode)
    {
        switch (resultCode)
        {
            case 0:
                return Ok(new { status = 200, message = "Successful" });
            case 10:
                return StatusCode(503, new { status = 503, message = "System is under maintenance. Please retry later." });
            case 11:
                return StatusCode(403, new { status = 403, message = "Access denied. Please check your settings in M4B portal, or contact MoMo for configurations." });
            case 12:
                return StatusCode(400, new { status = 400, message = "Unsupported API version. Please upgrade to the latest version of payment gateway." });
            case 13:
                return StatusCode(401, new { status = 401, message = "Merchant authentication failed. Please check your credentials." });
            case 20:
                return BadRequest(new { status = 400, message = "Bad format request. Please check the request format or any missing parameters." });
            case 21:
                return BadRequest(new { status = 400, message = "Invalid transaction amount. Please check the amount and retry." });
            case 22:
                return BadRequest(new { status = 400, message = "Transaction amount is out of range. Please check the allowed range of each payment method." });
            case 40:
                return BadRequest(new { status = 400, message = "Duplicated requestId. Please retry with a different requestId." });
            case 41:
                return BadRequest(new { status = 400, message = "Duplicated orderId. Please inquiry the orderId's transaction status, or retry with a different orderId." });
            case 42:
                return BadRequest(new { status = 400, message = "Invalid orderId or orderId not found. Please retry with a different orderId." });
            case 43:
                return BadRequest(new { status = 400, message = "Analogous transaction is being processed. Please check if another analogous transaction is being processed." });
            case 45:
                return BadRequest(new { status = 400, message = "Duplicated ItemId. Please retry with a unique ItemId." });
            case 47:
                return BadRequest(new { status = 400, message = "Inapplicable information in the given set of valuable data. Please review and retry with another request." });
            case 98:
                return StatusCode(503, new { status = 503, message = "This QR Code has not been generated successfully. Please retry later." });
            case 99:
                return StatusCode(500, new { status = 500, message = "Unknown error. Please contact MoMo for more details." });
            case 1000:
                return Ok(new { status = 200, message = "Transaction is initiated, waiting for user confirmation." });
            case 1001:
                return BadRequest(new { status = 400, message = "Transaction failed due to insufficient funds." });
            case 1002:
                return BadRequest(new { status = 400, message = "Transaction rejected by the issuers of the payment methods. Please choose other payment methods." });
            case 1003:
                return BadRequest(new { status = 400, message = "Transaction cancelled after successfully authorized." });
            case 1004:
                return BadRequest(new { status = 400, message = "Transaction failed because the amount exceeds daily/monthly payment limit. Please retry another day." });
            case 1005:
                return BadRequest(new { status = 400, message = "Transaction failed because the url or QR code expired. Please send another payment request." });
            case 1006:
                return BadRequest(new { status = 400, message = "Transaction failed because user has denied to confirm the payment. Please send another payment request." });
            case 1007:
                return BadRequest(new { status = 400, message = "Transaction rejected due to inactive or nonexistent user's account. Please ensure the account status should be active/verified before retrying." });
            case 1017:
                return BadRequest(new { status = 400, message = "Transaction cancelled by merchant." });
            case 1026:
                return BadRequest(new { status = 400, message = "Transaction restricted due to promotion rules. Please contact MoMo for details." });
            case 1080:
                return BadRequest(new { status = 400, message = "Refund attempt failed during processing. Please retry later." });
            case 1081:
                return BadRequest(new { status = 400, message = "Refund rejected. The original transaction might have been refunded or exceeds refundable amount." });
            case 2019:
                return BadRequest(new { status = 400, message = "Invalid orderGroupId. Please contact MoMo for details." });
            case 4001:
                return BadRequest(new { status = 400, message = "Transaction restricted due to incomplete KYCs. Please contact MoMo for details." });
            case 4100:
                return BadRequest(new { status = 400, message = "Transaction failed because user failed to login." });
            case 7000:
                return Ok(new { status = 200, message = "Transaction is being processed. Please wait for it to be fully processed." });
            case 7002:
                return Ok(new { status = 200, message = "Transaction is being processed by the provider of the selected payment instrument." });
            case 9000:
                return Ok(new { status = 200, message = "Transaction is authorized successfully. Please proceed with either capture or cancel request." });
            default:
                return StatusCode(500, new { status = 500, message = "Unhandled response code from MoMo. Please contact support." });
        }
    }
    public async Task<IActionResult> Buy_Success(string orderId)
    {
        var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
        try
        {
            if (existingOrder != null &&  existingOrder.State==false)
            {
                try
                {
                    await SendCodeActiveByEmail(existingOrder.Email, existingOrder.OrderId);

                }catch(Exception ex) { 
                   return BadRequest(ex.Message);
                }
                existingOrder.State = true;
                await _context.SaveChangesAsync();
                return Ok(new { status = 200, order = existingOrder, message = "Update Order State Successful and send email success" });
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
    public async Task<IActionResult> SendCodeActiveByEmail(string userEmail, string orderId)
    {
        try
        {
            var existOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            var codeActiveCourse = _smtpServices.GenerateCode(12);
            var subject = "[EStudy] - Thông  tin đơn hàng và mã kích hoạt khóa học";
            var userName =  await  _context.Users.Where(u=> u.UserId == existOrder.UserId).Select(u=> u.UserName).FirstOrDefaultAsync();
            var courseName = await _context.Courses.Where(u => u.CourseId == existOrder.CourseId).Select(u => u.CourseName).FirstOrDefaultAsync();
            try
            {
                var emailContent = _smtpServices.GenerateCodeByEmailContent(userName, existOrder.OrderDate.ToString(), orderId, courseName, codeActiveCourse);
                await _smtpServices.SendEmailAsync(userEmail, subject, emailContent, emailContent);

                if (existOrder != null || existOrder.State == true)
                {
                    existOrder.Code = codeActiveCourse;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred while sending the email or email not exist : {ex.Message}" });
            }
            return Ok("Email sent successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }    
  
  
}
