using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using study4_be.Helper;
using study4_be.Models;
using study4_be.Payment.MomoPayment;
using study4_be.PaymentServices.Momo.Config;
using study4_be.PaymentServices.Momo.Request;
using study4_be.Services;
using study4_be.Services.Payment;
using System.Security.Cryptography;
using System.Text;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankLinkController : ControllerBase
    {
        private readonly MomoConfig _momoConfig;
        private readonly MomoTestConfig _momoTestConfig;
        private readonly HashHelper _hashHelper;
        private readonly Study4Context _context;
        private HttpClient _httpClient = new();

        public BankLinkController(ILogger<BankLinkController> logger,
                                  IOptions<MomoConfig> momoPaymentSettings,
                                  SMTPServices smtpServices,
                                  ContractPOServices contractPOServices,
                                  Study4Context context, IOptions<MomoTestConfig> momoTestConfig)
        {
            _context = context;
            _hashHelper = new HashHelper();
            _momoConfig = momoPaymentSettings.Value;
            _momoTestConfig = momoTestConfig.Value;
        }

        [HttpPost("LinkWallet")]
        public async Task<IActionResult> LinkWallet([FromBody] BankLinkRequest request)
        {
            try
            {
                var signature = _hashHelper.GenerateSignature(request, _momoConfig);
                var response = await SendLinkPaymentRequest(request, signature);

                // Đọc nội dung phản hồi từ MoMo
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Phân tích nội dung phản hồi JSON để kiểm tra mã lỗi
                    var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);




                    if (jsonResponse != null && jsonResponse.TryGetValue("errorCode", out var errorCode) && errorCode.ToString() != "0")
                    {
                        // Có mã lỗi trong phản hồi, lấy thông báo lỗi chi tiết
                        var errorMessage = GetErrorMessage(errorCode.ToString());
                        return BadRequest(new { message = errorMessage });
                    }
                    return Ok(new { Message = "Gửi yêu cầu liên kết ví thành công", responseContent });
                }
                else
                {
                    // Khi mã HTTP không thành công
                    return BadRequest($"Yêu cầu thanh toán không thành công. Mã lỗi: {response.StatusCode}. Chi tiết lỗi: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi xử lý yêu cầu thanh toán: " + ex.Message);
            }
        }
        private async Task<HttpResponseMessage> SendLinkPaymentRequest(BankLinkRequest request, string signature)
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
                requestType = request.RequestType, // alway link wallet 
                extraData = request.ExtraData, // alway empty
                partnerClientId = request.partnerClientId,
                lang = request.Lang,
                signature = signature
            };
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(paymentData), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(_momoConfig.PaymentUrl, content);
        }
        [HttpPost("DecryptCallbackToken")]
        public async Task<IActionResult> DecryptCallbackToken([FromBody] DecryptAesTokenRequest request)
        {
            try
            {
                // Gửi yêu cầu để nhận AES token từ Momo
                var aesTokenResponse = await SendAesTokenizationRequest(request);
                var responseContent = await aesTokenResponse.Content.ReadAsStringAsync();

                if (!aesTokenResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)aesTokenResponse.StatusCode, "Failed to get AES token from Momo." + responseContent);
                }

                // Đọc nội dung phản hồi và lấy aesToken
                dynamic responseJson = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
                string aesToken = responseJson?.aesToken;

                if (string.IsNullOrEmpty(aesToken))
                {
                    return BadRequest("aesToken not found in the response.");
                }

                // Giải mã callbackToken
                var decryptedToken = DecryptAES(_momoConfig.SecretKey, aesToken);
                

                // right here should save aesToken

                // Trả về kết quả
                return Ok(new { Message = "Giải mã thành công", DecryptedToken = decryptedToken });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về phản hồi lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi giải mã token: " + ex.Message);
            }
        }
        private static string DecryptAES(string secretKey, string encryptedData)
        {
            try
            {
                // Tạo IV (Initialization Vector) bằng mảng byte 16
                byte[] iv = new byte[16];
                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(secretKey);
                    aes.IV = iv;

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData)))
                    using (var cryptoStream = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(cryptoStream))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và ghi log nếu cần
                Console.WriteLine($"Error decrypting data: {ex.Message}");
                return string.Empty;
            }
        }
        private async Task<HttpResponseMessage> SendAesTokenizationRequest(DecryptAesTokenRequest request)
        {
            // Tạo chữ ký
            var signature = _hashHelper.GenerateSignature(request, _momoConfig);

            // Dữ liệu yêu cầu gửi lên Momo
            var paymentData = new
            {
                partnerCode = _momoConfig.PartnerCode,
                callbackToken = request.CallbackToken,
                requestId = request.RequestId,
                amount = request.Amount,
                orderId = request.OrderId,
                partnerClientId = request.PartnerClientId,
                lang = request.Lang,
                signature = signature,
            };

            // Gửi yêu cầu POST đến API Momo
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(paymentData), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(_momoConfig.AesTokenUrl, content);
        }


        private string GetErrorMessage(string errorCode)
        {
            return Int32.Parse(errorCode) switch
            {
                0 => "Thành công.",
                2001 => "Giao dịch thất bại do sai thông tin liên kết. Token liên kết không tồn tại hoặc đã bị xóa, vui lòng cập nhật dữ liệu của bạn.",
                2007 => "Giao dịch thất bại do liên kết hiện đang bị tạm khóa. Token liên kết hiện đang ở trạng thái không hoạt động.",
                2012 => "Yêu cầu bị từ chối vì token không khả dụng. Token không tồn tại hoặc đã bị xóa.",
                3001 => "Liên kết thất bại do người dùng từ chối xác nhận.",
                3002 => "Liên kết bị từ chối do không thỏa quy tắc liên kết. PartnerClientId đã được liên kết với tài khoản MoMo khác.",
                3003 => "Hủy liên kết bị từ chối do đã vượt quá số lần hủy. Vui lòng liên hệ MoMo để biết thêm chi tiết.",
                3004 => "Liên kết không thể hủy do có giao dịch đang chờ xử lý.",
                8000 => "Giao dịch đang ở trạng thái cần được người dùng xác nhận thanh toán lại.",
                9000 => "Giao dịch đã được xác nhận thành công.",
                _ => "Đã xảy ra lỗi không xác định.",
            };
        }

        [HttpPost("PaymentNotification")]
        public async Task<IActionResult> PaymentNotificationIpn([FromBody] PaymentNotification notification)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(notification.OrderId) || string.IsNullOrEmpty(notification.RequestId))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }
            // Kiểm tra mã kết quả và thực hiện hành động tương ứng
            var errorMessage = GetErrorMessage(notification.ResultCode.ToString());
            switch (notification.ResultCode)
            {
                case 0:
                    return Ok($"Giao dịch thành công: {errorMessage}");

                case 110:
                    return BadRequest($"Giao dịch không thành công: {errorMessage}");

                default:
                    return StatusCode(500, $"Lỗi không xác định: {errorMessage}");
            }
        }

        [HttpPost("Disbursement")]
        public async Task<IActionResult> Disbursement([FromBody] DisbursementRequest request)
        {
            try
            {
                // Dữ liệu yêu cầu giải ngân
                string publicKey = _momoTestConfig.PublicKey;
                string disbursementMethod = EncryptDisbursementData(JsonConvert.SerializeObject(request), CreateRSAFromPublicKey(publicKey));
                // Tạo chữ ký (signature) cho yêu cầu giải ngân
                var signature = _hashHelper.GenerateDisbursementSignature(request, _momoTestConfig, disbursementMethod);

                // Gửi yêu cầu giải ngân bằng phương thức sendDisbursementRequest
                var response = await SendDisbursementRequest(request, signature, disbursementMethod);

                // Đọc nội dung phản hồi từ Momo
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode || ((int)response.StatusCode) == 11)
                {
                    // Phản hồi thành công
                    var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    return Ok(new {
                        Message = "Giải ngân thành công", 
                        jsonResponse,
                        statusCode = 200
                    });
                }
                else
                {
                    // Phản hồi thất bại
                    return BadRequest($"Yêu cầu giải ngân không thành công. Mã lỗi: {response.StatusCode}. Chi tiết: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và ghi log
                return StatusCode(500, "Đã xảy ra lỗi khi thực hiện giải ngân: " + ex.Message);
            }
        }
        private async Task<HttpResponseMessage> SendDisbursementRequest(DisbursementRequest request, string signature, string disbursementMethod)
        {

            var disbursementData = new
            {
                partnerCode = _momoTestConfig.PartnerCode,
                orderId = request.OrderId,
                amount = request.Amount,
                requestId = request.RequestId,
                requestType = request.RequestType, /*disburseToWallet || disburseToBank*/
                disbursementMethod = disbursementMethod,
                extraData = request.ExtraData,
                orderInfo = request.OrderInfo,
                partnerClientId = request.PartnerClientId, // sdt
                lang = request.Lang,
                signature = signature
            };

            // Chuyển đổi dữ liệu sang JSON và tạo nội dung yêu cầu
            var content = new StringContent(JsonConvert.SerializeObject(disbursementData), Encoding.UTF8, "application/json");

            // Gửi yêu cầu POST đến API của Momo (Disbursement API URL)
            return await _httpClient.PostAsync(_momoTestConfig.DisbursementUrl, content);
        }
        private static RSA CreateRSAFromPublicKey(string publicKeyPEM)
        {
            // Kiểm tra nếu khóa công khai là rỗng
            if (string.IsNullOrWhiteSpace(publicKeyPEM))
            {
                Console.WriteLine("Môi trường test: Khóa công khai rỗng. Tạo khóa RSA ngẫu nhiên.");

                // Tạo và trả về một đối tượng RSA ngẫu nhiên trong môi trường test
                return RSA.Create();
            }

            // Loại bỏ phần header và footer khỏi PEM public key
            var publicKey = publicKeyPEM
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Trim();

            // Giải mã chuỗi base64
            byte[] keyBytes = Convert.FromBase64String(publicKey);

            // Tạo đối tượng RSA
            RSA rsa = RSA.Create();

            // Nhập thông tin khóa công khai
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

            return rsa;
        }
        private static string EncryptDisbursementData(string data, RSA rsa)
        {
            // Convert the input string to a byte array
            byte[] dataToEncrypt = System.Text.Encoding.UTF8.GetBytes(data);

            // Encrypt the data
            byte[] encryptedData = rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);

            // Return the encrypted data as a base64 string
            return Convert.ToBase64String(encryptedData);
        }

    }
}   
