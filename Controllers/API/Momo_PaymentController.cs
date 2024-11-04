using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using study4_be.Helper;
using study4_be.Models;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.Validation;
using System;

using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using study4_be.Services;
using Microsoft.EntityFrameworkCore;
using study4_be.Services.Payment;
using study4_be.PaymentServices.Momo.Request;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Momo_PaymentController : ControllerBase
    {
        private readonly ILogger<Momo_PaymentController> _logger;
        private readonly MomoConfig _momoConfig;
        private readonly HashHelper _hashHelper;
        private readonly ContractPOServices _contractPOServices;
        private readonly SMTPServices _smtpServices;
        private readonly Study4Context _context;
        private HttpClient _httpClient = new();
        public Momo_PaymentController(ILogger<Momo_PaymentController> logger,
                                     IOptions<MomoConfig> momoPaymentSettings,
                                     SMTPServices sMTPServices,
                                     ContractPOServices contractPOServices,
                                     Study4Context context)
        {
            _context = context;
            _logger = logger;
            _hashHelper = new HashHelper();
            _momoConfig = momoPaymentSettings.Value;
            _smtpServices = sMTPServices;
            _contractPOServices = contractPOServices; // DI will inject this
        }

        [HttpPost("MakePayment")]
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
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(paymentData), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(_momoConfig.PaymentUrl, content);
        }

        [HttpPost("GetIpnFromMomo")]
        public async Task<IActionResult> GetIpnFromMomo([FromBody] GetMomoIPNResult resultIpn)
        {
            int resultCode = resultIpn.resultCode;
            string orderString = resultIpn.orderId;
            if (resultCode == 0 || resultCode == 9000)
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
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(dataRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(aa, content);
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

        [HttpPost("test")]
        public async Task<IActionResult> Buy_Success([FromQuery] string orderId)
        {
            try
            {
                // Tìm order với orderId và kiểm tra course_id khác null
                var existingOrderCourse = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CourseId != null);

                if (existingOrderCourse != null)
                {
                    return await HandleCourseOrder(existingOrderCourse);
                }

                // Tìm order với orderId và kiểm tra plan_id khác null
                var newOrderPlan = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.PlanId != null);

                if (newOrderPlan != null)
                {
                    return await HandleSubscriptionPlan(newOrderPlan);
                }

                var existingOrderDocument = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.DocumentId != null);

                if (existingOrderDocument != null)
                {
                    return await HandleDocument(existingOrderDocument);
                }
                return BadRequest("Order not found");
            }
            catch (Exception e)
            {
                return BadRequest($"Error updating order state: {e.Message}");
            }
        }

        private async Task<IActionResult> HandleCourseOrder(Order existingOrderCourse)
        {
            if (existingOrderCourse.State == false)
            {
                var existingUserCourse = await _context.UserCourses.FindAsync(existingOrderCourse.UserId, existingOrderCourse.CourseId);
                if (existingUserCourse == null)
                {
                    return await HandleBuyNewCourse(existingOrderCourse);
                }
                else
                {
                    return await HandleRenewCourse(existingOrderCourse, existingUserCourse);
                }
            }

            return BadRequest("You Had Bought Before");
        }

        private async Task<IActionResult> HandleBuyNewCourse(Order existingOrderCourse)
        {
            existingOrderCourse.State = true;
            await _context.SaveChangesAsync();
            await SendCodeActiveByEmail(existingOrderCourse.Email, existingOrderCourse.OrderId);
            var respone = new
            {
                existingOrderCourse.OrderId,
                existingOrderCourse.UserId,
                existingOrderCourse.CourseId,
                existingOrderCourse.OrderDate,
                existingOrderCourse.State,
                existingOrderCourse.TotalAmount,
                existingOrderCourse.Code,
                existingOrderCourse.Email,

            };
            return Ok(new
            {
                status = 200,
                order = respone,
                message = "Update Order State Successful and send email success"
            });
        }
        private async Task<IActionResult> HandleRenewCourse(Order existingOrderCourse, UserCourse existingUserCourse)
        {
            existingOrderCourse.State = true;
            var newUserCourse = new UserCourse
            {
                UserId = existingUserCourse.UserId,
                CourseId = (int)existingUserCourse.CourseId,
                Date = DateTime.Now,
                Process = existingUserCourse.Process,
                State = true
            };
            _context.UserCourses.RemoveRange(existingUserCourse);
            await _context.UserCourses.AddRangeAsync(newUserCourse);
            await _context.SaveChangesAsync();

            var respone = new
            {
                existingOrderCourse.OrderId,
                existingOrderCourse.UserId,
                existingOrderCourse.CourseId,
                existingOrderCourse.OrderDate,
                existingOrderCourse.State,
                existingOrderCourse.TotalAmount

            };
            return Ok(new
            {
                status = 200,
                order = respone,
                message = "Update Order State Successful and renew course"
            });
        }
        private async Task<IActionResult> HandleSubscriptionPlan(Order newOrderPlan)
        {
            if (newOrderPlan.State == false)
            {
                // Find the existing subscription plan details
                var existingPlan = await _context.Subscriptionplans.FindAsync(newOrderPlan.PlanId);
                if (existingPlan == null)
                {
                    return NotFound(new { message = "Subscription plan not found" });
                }

                // Update the order state to indicate successful payment
                newOrderPlan.State = true;

                // Create a new user subscription record
                var newUserSub = new UserSub
                {
                    UserId = newOrderPlan.UserId,
                    PlanId = (int)newOrderPlan.PlanId,
                    UsersubsStartdate = DateTime.Now,
                    UsersubsEnddate = DateTime.Now.AddDays(existingPlan.PlanDuration),
                    State = true,
                };

                // Find the old subscription for the user, if it exists
                var oldPlan = await _context.UserSubs
                                             .FirstOrDefaultAsync(us => us.UserId == newUserSub.UserId && us.State == true);

                // Remove the old plan if it exists
                if (oldPlan != null)
                {
                    oldPlan.State = false;
                }

                // Add the new user subscription
                await _context.UserSubs.AddAsync(newUserSub);

                // Retrieve the courses associated with the plan from PLAN_COURSE
                var planCourses = await _context.PlanCourses
                                                .Where(pc => pc.PlanId == newOrderPlan.PlanId)
                                                .ToListAsync();

                // Add each course to the USER_COURSES table for the user
                foreach (var planCourse in planCourses)
                {
                    bool courseExists = await _context.UserCourses
                                                      .AnyAsync(uc => uc.UserId == newOrderPlan.UserId && uc.CourseId == planCourse.CourseId);

                    if (!courseExists)
                    {
                        var userCourse = new UserCourse
                        {
                            UserId = newOrderPlan.UserId,
                            CourseId = planCourse.CourseId,
                            Process = 0,
                            Date = DateTime.Now,
                            State = true,
                        };
                        await _context.UserCourses.AddAsync(userCourse);
                    }
                }

                // Save all changes in one transaction
                await _context.SaveChangesAsync();
               
                var respon = new
                {
                    newOrderPlan.OrderId,
                    newOrderPlan.PlanId,
                    newOrderPlan.UserId,
                    newOrderPlan.TotalAmount,
                    newOrderPlan.CreatedAt,
                    newOrderPlan.State
                };

                await SendPaymentConfirmationByEmail(newOrderPlan.OrderId);
                return Ok(new
                {
                    status = 200,
                    order = respon,
                    message = "Subscription plan renewed and order state updated successfully"
                });
            }

            // If the order state was already true, return a bad request
            return BadRequest(new { message = "Order is already completed or invalid" });
        }
        private async Task<IActionResult> HandleDocument(Order existingOrderDocument)
        {
            if (existingOrderDocument.State == false)
            {
                existingOrderDocument.State = true;
                var newUserDocument = new UserDocument
                {
                    DocumentId = (int)existingOrderDocument.DocumentId,
                    UserId = existingOrderDocument.UserId,
                    OrderDate = DateTime.Now,
                    State = true,
                };
                var existingDocument = await _context.Documents.FindAsync(existingOrderDocument.DocumentId);
            
                var userUploaded = await _context.Users.FindAsync(existingDocument.UserId);

                userUploaded.Blance += existingOrderDocument.TotalAmount;

                await _context.UserDocuments.AddAsync(newUserDocument);
                await _context.SaveChangesAsync();
                var respone = new
                {
                    existingOrderDocument.OrderId,
                    existingOrderDocument.DocumentId,
                    existingOrderDocument.UserId,
                    existingOrderDocument.OrderDate,
                    existingOrderDocument.TotalAmount,
                    existingOrderDocument.State,
                };
                await SendPaymentConfirmationByEmail(existingOrderDocument.OrderId);
                return Ok(new
                {
                    status = 200,
                    order = respone,
                    message = "Update Order State Successful and send email success"
                });
            }
            return BadRequest(new { message = "Order is already completed or invalid" });
        }

        private async Task<IActionResult> SendPaymentConfirmationByEmail(string orderId)
        {
            try
            {
                var existOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (existOrder == null)
                {
                    return NotFound(new { status = 404, message = $"Order with ID {orderId} not found." });
                }

                // Generate PDF and get data
                string invoiceResult = await _contractPOServices.GenerateInvoicePdf(orderId);
                if (invoiceResult == null)
                {
                    return NotFound(new { status = 404, message = "Had a problem with creating the invoice." });
                }

                var subject = "[EStudy] - Payment Confirmation for Your Order";
                var userName = await _context.Users.Where(u => u.UserId == existOrder.UserId).Select(u => u.UserName).FirstOrDefaultAsync();
                var userEmail = await _context.Users.Where(u => u.UserId == existOrder.UserId).Select(u => u.UserEmail).FirstOrDefaultAsync();
                // Check if the order is for a document or a subscription plan
                var documentName = await _context.Documents.Where(d => d.DocumentId == existOrder.DocumentId).Select(d => d.Title).FirstOrDefaultAsync();
                var planName = await _context.Subscriptionplans.Where(p => p.PlanId == existOrder.PlanId).Select(p => p.PlanName).FirstOrDefaultAsync();

                string emailContent;

                // Generate email content based on the order type
                if (documentName != null)
                {
                    emailContent = _smtpServices.GenerateDocumentPaymentEmailContent(userName, existOrder.OrderDate.ToString(), orderId, documentName, invoiceResult);
                }
                else if (planName != null)
                {
                    emailContent = _smtpServices.GeneratePlanPaymentEmailContent(userName, existOrder.OrderDate.ToString(), orderId, planName, invoiceResult);
                }
                else
                {
                    return NotFound(new { status = 404, message = "No valid document or plan found for the given order." });
                }

                // Attempt to send the email
                await _smtpServices.SendEmailAsync(userEmail, subject, emailContent, emailContent);

                return Ok(new { status = 200, message = "Payment confirmation email sent successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, new { status = 500, message = $"An error occurred while sending the payment confirmation email: {ex.Message}" });
            }
        }

        private async Task<IActionResult> SendCodeActiveByEmail(string userEmail, string orderId)
        {
            try
            {
                var existOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (existOrder == null)
                {
                    return NotFound(new { status = 404, message = $"Order with ID {orderId} not found." });
                }

                // Generate PDF and get data
                string invoiceResult = await _contractPOServices.GenerateInvoicePdf(orderId);
                if (invoiceResult == null)
                {
                    return NotFound(new { status = 404, message = "Had problem with create invoice " });
                }
                var codeActiveCourse = _smtpServices.GenerateCode(12);
                var subject = "[EStudy] - Thông tin đơn hàng và mã kích hoạt khóa học";
                var userName = await _context.Users.Where(u => u.UserId == existOrder.UserId).Select(u => u.UserName).FirstOrDefaultAsync();
                var courseName = await _context.Courses.Where(u => u.CourseId == existOrder.CourseId).Select(u => u.CourseName).FirstOrDefaultAsync();
                // Generate email content with the invoice URL
                var emailContent = _smtpServices.GenerateCodeByEmailContent(userName, existOrder.OrderDate.ToString(), orderId, courseName, codeActiveCourse, invoiceResult);

                // Attempt to send the email
                await _smtpServices.SendEmailAsync(userEmail, subject, emailContent, emailContent);

                // Update order state with the activation code
                if (existOrder.State == true)
                {
                    existOrder.Code = codeActiveCourse;
                    await _context.SaveChangesAsync();
                }

                return Ok(new { status = 200, message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, new { status = 500, message = $"An error occurred while sending the email: {ex.Message}" });
            }
        }
        public async Task<IActionResult> SendCodeActiveByEmail(LogTestRequest _req) // unit test 
        {
            try
            {
                var existOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == _req.orderId);
                if (existOrder == null)
                {
                    return NotFound(new { status = 404, message = $"Order with ID {_req.orderId} not found." });
                }

                // Generate PDF and get data
                string invoiceResult = await _contractPOServices.GenerateInvoicePdf(_req.orderId);
                if (invoiceResult == null)
                {
                    return NotFound(new { status = 404, message = "Had problem with create invoice " });
                }
                var codeActiveCourse = _smtpServices.GenerateCode(12);
                var subject = "[EStudy] - Thông tin đơn hàng và mã kích hoạt khóa học";
                var userName = await _context.Users.Where(u => u.UserId == existOrder.UserId).Select(u => u.UserName).FirstOrDefaultAsync();
                var courseName = await _context.Courses.Where(u => u.CourseId == existOrder.CourseId).Select(u => u.CourseName).FirstOrDefaultAsync();

                // Generate email content with the invoice URL
                var emailContent = _smtpServices.GenerateCodeByEmailContent(userName, existOrder.OrderDate.ToString(), _req.orderId, courseName, codeActiveCourse, invoiceResult);

                // Attempt to send the email
                await _smtpServices.SendEmailAsync(_req.userEmail, subject, emailContent, emailContent);

                // Update order state with the activation code
                if (existOrder.State == true)
                {
                    return StatusCode(500, new { status = 500, message = $"An error occurred while sending the email" });
                }

                return Ok(new { status = 200, message = "Email sent successfully" });
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                return StatusCode(500, new { status = 500, message = $"An error occurred while sending the email: {ex.Message}" });
            }
        }

    }
}
