using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
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
        [HttpGet("GetAllWallets")]
        public async Task<ActionResult> GetAllWallets()
        {
            var wallets = await _context.Wallets.ToListAsync();
            return Ok(new { status = 200, message = "Get Wallets Successful", wallets });

        }

        [HttpPost("LinkWallet")]
        public async Task<IActionResult> LinkWallet([FromBody] BankLinkRequest request)
        {
            try
            {
                request.OrderId = Guid.NewGuid().ToString();
                request.RequestId = Guid.NewGuid().ToString();
                var signature = _hashHelper.GenerateSignature(request, _momoConfig);
                var response = await SendLinkPaymentRequest(request, signature);

                // Đọc nội dung phản hồi từ MoMo
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Phân tích nội dung phản hồi JSON để kiểm tra mã lỗi
                    var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    var wallet = new Wallet
                    {
                        Id = Guid.NewGuid().ToString(),
                        CardNumber = request.partnerClientId,
                        IsAvailable = false,
                        Name = request.UserInfo.WalletName,
                        Userid = request.UserInfo.UserId,
                        Type = request.RequestType,
                    };
                    if (jsonResponse != null && jsonResponse.TryGetValue("errorCode", out var errorCode) && errorCode.ToString() != "0")
                    {
                        // Có mã lỗi trong phản hồi, lấy thông báo lỗi chi tiết
                        var errorMessage = GetErrorMessage(errorCode.ToString());
                        return BadRequest(new { message = errorMessage });
                    }
                    await _context.AddAsync(wallet);
                    await _context.SaveChangesAsync();
                    var walletResponse = new
                    {
                        walletId = wallet.Id,
                        cardNumber = wallet.CardNumber,
                        isAvailable = wallet.IsAvailable,
                        name = wallet.Name,
                        userId = wallet.Userid,
                        type = wallet.Type,
                    };
                    return Ok(new { statusCode =200 ,message = "Gửi yêu cầu liên kết ví thành công", responseContent, walletResponse });
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
                requestId =request.RequestId,
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

        private string GenerateOrderId(string userId, int walletId)
        {
            using (var sha256 = SHA256.Create())
            {
                var baseString = $"{userId}-{walletId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                return ToBase32String(hashBytes).Substring(0, 32); // Increase length to 32 characters
            }
        }

        [HttpGet]
        private string ToBase32String(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXY1Z23456789";
            StringBuilder result = new StringBuilder((bytes.Length + 4) / 5 * 8);
            int bitIndex = 0;
            int currentByte = 0;

            while (bitIndex < bytes.Length * 8)
            {
                if (bitIndex % 8 == 0)
                {
                    currentByte = bytes[bitIndex / 8];
                }

                int dualByte = currentByte << 8;
                if ((bitIndex / 8) + 1 < bytes.Length)
                {
                    dualByte |= bytes[(bitIndex / 8) + 1];
                }

                int index = (dualByte >> (16 - (bitIndex % 8 + 5))) & 31;
                result.Append(alphabet[index]);

                bitIndex += 5;
            }

            return result.ToString();
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
            if (string.IsNullOrEmpty(notification.OrderId) || string.IsNullOrEmpty(notification.RequestId )|| string.IsNullOrEmpty(notification.walletId))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }
            // Kiểm tra mã kết quả và thực hiện hành động tương ứng
            var errorMessage = GetErrorMessage(notification.ResultCode.ToString());
            switch (notification.ResultCode)
            {
                case 0:
                    var (success, message, wallet) = await SaveWallet(notification.walletId);
                    if (!success)
                    {
                        return BadRequest($"Không thể lưu ví: {message}");
                    }
                    return Ok(new
                    {
                        message = "Giao dịch thành công.",
                        statusCode = 200,
                        wallet
                    });

                case 110:
                    return BadRequest($"Giao dịch không thành công: {errorMessage}");

                default:
                    return StatusCode(500, $"Lỗi không xác định: {errorMessage}");
            }
        }
        private async Task<(bool Success, string Message, object walletData)> SaveWallet(string walletId)
        {
            if (string.IsNullOrEmpty(walletId))
            {
                return (false, "WalletId không được để trống.", null);
            }

            var existWallet = await _context.Wallets.FindAsync(walletId);
            if (existWallet == null)
            {
                return (false, "Wallet không tồn tại.", null);
            }

            if (existWallet.IsAvailable)
            {
                return (false, "Wallet đã được liên kết trước đó.", null);
            }

            existWallet.IsAvailable = true;
            await _context.SaveChangesAsync();
            var walletResponse = new
            {
                walletId = existWallet.Id,
                cardNumber = existWallet.CardNumber,
                isAvailable = existWallet.IsAvailable,
                name = existWallet.Name,
                userId = existWallet.Userid,
                type = existWallet.Type,
            };
            // Trả về đối tượng Wallet trong JSON khi thành công
            return (true, "Xác thực ví thành công.", walletResponse);
        }
        [HttpGet("GetUserWallets/{userId}")]
        public async Task<IActionResult> GetUserWallets(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId không được để trống.");
            }

            // Truy vấn tất cả các Wallet của người dùng với userId tương ứng
            var userWallets = await _context.Wallets
                                            .Where(wallet => wallet.Userid == userId)
                                            .Select(wallet => new
                                            {
                                                walletId = wallet.Id,
                                                cardNumber = wallet.CardNumber,
                                                isAvailable = wallet.IsAvailable,
                                                name = wallet.Name,
                                                userId = wallet.Userid,
                                                type = wallet.Type
                                            })
                                            .ToListAsync();

            if (userWallets == null || !userWallets.Any())
            {
                return NotFound("Không tìm thấy ví nào cho người dùng này.");
            }

            return Ok(new { statusCode = 200, message = "Lấy danh sách ví thành công", data = userWallets });
        }
        [HttpPost("TestAddBalance")]
        public async Task<IActionResult> TestAddBalance([FromBody] TestAddBalanceRequest request)
        {
            try
            {
                // Kiểm tra số tiền phải lớn hơn 0
                if (request.Amount <= 0)
                {
                    return BadRequest("Số tiền phải lớn hơn 0.");
                }

                // Tìm người dùng dựa vào userId
                var existUser = await _context.Users.FindAsync(request.UserId);
                if (existUser == null)
                {
                    return NotFound($"Không tìm thấy người dùng với ID: {request.UserId}");
                }

                // Thêm số tiền vào balance của người dùng
                if (existUser.Blance == null)
                {
                    existUser.Blance = 0;
                }
                existUser.Blance += request.Amount;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Cập nhật số tiền thành công",
                    statusCode = 200,
                    NewBalance = existUser.Blance
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Đã xảy ra lỗi khi thêm số tiền: " + ex.Message);
            }
        }
        public class TestAddBalanceRequest
        {
            public string UserId { get; set; }
            public double Amount { get; set; }
        }
        public class DisbursementMethodRequest
        {
            public string RequestId { get; set; } // ID yêu cầu duy nhất
            public long Amount { get; set; } // Số tiền giải ngân
            public string OrderId { get; set; } // Mã đơn hàng
            public string PartnerClientId { get; set; } // ID khách hàng của partner
            public string Lang { get; set; } // Ngôn ngữ (VD: "vi" hoặc "en")
            public string RequestType { get; set; }
            public string OrderInfo { get; set; }
            public string ExtraData { get; set; }
         
        }
        [HttpPost("Disbursement")]
        public async Task<IActionResult> Disbursement([FromBody] DisbursementRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return BadRequest("Yêu cầu giải ngân không thành công. Số tiền phải lớn hơn 0.");
                }

                var existUser = await _context.Users.FindAsync(request.userId);
                if (existUser == null)
                {
                    return BadRequest($"Không tìm thấy người dùng với ID: {request.userId}.");
                }

                // Nếu số dư là null, khởi tạo nó về 0
                if (existUser.Blance == null)
                {
                    existUser.Blance = 0;
                    await _context.SaveChangesAsync();
                }

                // Tính số tiền thực tế sau thuế
                long amountAfterTax = request.Amount * 90 / 100; // Giả sử thuế là 10%

                // Kiểm tra xem người dùng có đủ số dư không
                if (amountAfterTax > existUser.Blance)
                {
                    return BadRequest($"Yêu cầu giải ngân không thành công. Số dư hiện tại: {existUser.Blance}, Số tiền sau thuế: {amountAfterTax}, Số tiền yêu cầu: {request.Amount}. Không đủ tiền.");
                }

                // Cập nhật số dư của người dùng
                existUser.Blance -= amountAfterTax; // Trừ số tiền thực tế sau thuế
                request.Amount = amountAfterTax;
                // Dữ liệu yêu cầu giải ngân (bao gồm cả thông tin người dùng)
                string publicKey = _momoConfig.PublicKey;

                // Tạo một đối tượng mới không có userId và walletId
                var disbursementData = new DisbursementMethodRequest
                {
                    RequestId = request.RequestId,
                    Amount = amountAfterTax,
                    OrderId = request.OrderId,
                    PartnerClientId = request.PartnerClientId,
                    Lang = request.Lang,
                    RequestType = request.RequestType,
                    OrderInfo = request.OrderInfo,
                    ExtraData = request.ExtraData,
                };

                // Mã hóa dữ liệu yêu cầu giải ngân
                string disbursementMethod = EncryptDisbursementData(JsonConvert.SerializeObject(disbursementData), CreateRSAFromPublicKey(publicKey));

                // Tạo chữ ký (signature) cho yêu cầu giải ngân
                var signature = _hashHelper.GenerateDisbursementSignature(request, _momoConfig, disbursementMethod);

                // Gửi yêu cầu giải ngân
                var response = await SendDisbursementRequest(request, signature, disbursementMethod);
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);

                if (response.IsSuccessStatusCode || (jsonResponse.TryGetValue("resultCode", out var resultCode) && (resultCode.ToString() == "11")))
                {
                    // Tạo đơn hàng mới
                    var saveOrder = new Order
                    {
                        OrderId = request.OrderId,
                        CreatedAt = DateTime.UtcNow,
                        OrderDate = DateTime.UtcNow,
                        PaymentType = "WithDraw",
                        State = true,
                        TotalAmount = amountAfterTax, // Lưu lại số tiền thực tế đã giải ngân
                        WalletId = request.walletId, // Lưu lại walletId của người dùng
                        UserId = existUser.UserId, // Lưu lại userId của người dùng
                    };

                    await _context.AddAsync(saveOrder);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Giải ngân thành công",
                        statusCode = 200,
                        saveOrder,
                        existUser,
                    });
                }
                else
                {
                    return BadRequest($"Yêu cầu giải ngân không thành công. Mã lỗi: {response.StatusCode}. Chi tiết: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                // Ghi log và trả về mã lỗi
                return StatusCode(500, "Đã xảy ra lỗi khi thực hiện giải ngân: " + ex.Message);
            }
        }


        private async Task<HttpResponseMessage> SendDisbursementRequest(DisbursementRequest request, string signature, string disbursementMethod)
        {

            var disbursementData = new
            {
                partnerCode = _momoConfig.PartnerCode,
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
            return await _httpClient.PostAsync(_momoConfig.DisbursementUrl, content);
        }
        public static RSA CreateRSAFromPublicKey(string publicKeyPEM)
        {
            // Remove the header and footer from the PEM public key
            var publicKey = publicKeyPEM
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Trim();

            // Decode the base64 string
            byte[] keyBytes = Convert.FromBase64String(publicKey);

            // Create a new RSA object
            RSA rsa = RSA.Create();

            // Import the public key information
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

            return rsa;
        }

        public static string EncryptDisbursementData(string data, RSA rsa)
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
