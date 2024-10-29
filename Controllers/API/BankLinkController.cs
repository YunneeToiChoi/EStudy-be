using System.Net;
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
using study4_be.PaymentServices.Bank.Request;
using study4_be.PaymentServices.Bank.Validator;
using study4_be.Services.Tingee;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankLinkController : ControllerBase
    {
        private readonly MomoConfig _momoConfig;
        private readonly HashHelper _hashHelper;
        private readonly Study4Context _context;
        private readonly TingeeApi _tingeeApi;
        private string bankUrl;
        private string logoMomo;
        private HttpClient _httpClient = new();

        public BankLinkController(ILogger<BankLinkController> logger,
                                  IOptions<MomoConfig> momoPaymentSettings,
                                  SMTPServices smtpServices,
                                  ContractPOServices contractPOServices,
                                  Study4Context context, IConfiguration configuration,
                                  TingeeApi tingeeApi)
        {
            _context = context;
            _hashHelper = new HashHelper();
            _momoConfig = momoPaymentSettings.Value;
            _tingeeApi = tingeeApi;
            bankUrl = configuration["Tingee:ApiUrl:BankUrl"];
            logoMomo = configuration["Momo:Logo"];
        }
        [HttpGet("GetAllWallets")]
        public async Task<ActionResult> GetAllWallets()
        {
            var wallets = await _context.Wallets.ToListAsync();
            return Ok(new { status = 200, message = "Get Wallets Successful", wallets });

        }
        [HttpGet("GetAllBanks")]
        public async Task<ActionResult> GetAllBanks([FromServices] IHttpClientFactory httpClientFactory)
        {
            var httpClient = httpClientFactory.CreateClient();

            try
            {
                var response = await httpClient.GetAsync("https://api.vietqr.io/v2/banks");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Lỗi khi lấy dữ liệu ngân hàng từ API.");
                }

                var resultContent = await response.Content.ReadAsStringAsync();

                // Giải mã dữ liệu JSON
                var bankResponse = JsonConvert.DeserializeObject<BankResponse>(resultContent);

                // Kiểm tra và sửa code nếu cần
                if (bankResponse.data != null)
                {
                    foreach (var bank in bankResponse.data)
                    {
                        if (bank.code == "MB")
                        {
                            bank.code = "MBB"; // Sửa code
                        }
                    }
                }

                // Trả về dữ liệu cho người dùng
                return Ok(bankResponse.data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public class BankResponse
        {
            public int code { get; set; }
            public List<Bank> data { get; set; }
        }
        [HttpGet("GetUserWallets/{userId}")]
        public async Task<IActionResult> GetUserWallets(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId không được để trống.");
                }

                var userExist = await _context.Users.FindAsync(userId);
                if (userExist == null)
                {
                    return BadRequest("UserId không tồn tại");
                }
                // Truy vấn tất cả các Wallet của người dùng với userId tương ứng
                var userWallets = await _context.Wallets
                    .Where(wallet => wallet.Userid == userId && wallet.IsAvailable == true)
                    .Select(wallet => new
                    {
                        walletId = wallet.Id,
                        walletImage = wallet.WalletImage,
                        cardNumber = wallet.CardNumber,
                        isAvailable = wallet.IsAvailable,
                        name = wallet.Name,
                        userId = wallet.Userid,
                        userBlance = userExist.Blance,
                        type = wallet.Type,
                    })
                    .ToListAsync();

                if (userWallets == null || !userWallets.Any())
                {
                    return NotFound("Không tìm thấy ví nào cho người dùng này.");
                }

                return Ok(new { statusCode = 200, message = "Lấy danh sách ví thành công", data = userWallets });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }   
        [HttpGet("HistoryPayment/{userId}")]
        public async Task<IActionResult> HistoryPayment(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId không được để trống.");
                }

                var userExist = await _context.Users.FindAsync(userId);
                if (userExist == null)
                {
                    return BadRequest("UserId không tồn tại");
                }
                var ord = await _context.Orders.Where(u=> u.UserId == userId && u.State == true).ToListAsync(); ;
                return Ok(new { statusCode = 200, message = "Lấy danh sách lịch sử giao dịch thành công", data = ord });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
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
                        WalletImage = logoMomo,
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
                    return Ok(new { statusCode = 200, message = "Gửi yêu cầu liên kết ví thành công", responseContent, walletResponse });
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
        [HttpPost("RemoveWallet")]
        public async Task<IActionResult> RemoveWallet(RemoveWalletRequest __req)
        {
            // Check if user exists
            var userExist = await _context.Users.FindAsync(__req.userId);
            if (userExist == null) 
                return BadRequest("User không tồn tại");

            // Find the specific wallet for the user
            var walletExist = await _context.Wallets.SingleOrDefaultAsync(w => w.Userid == __req.userId && w.Id == __req.walletId);
            if (walletExist == null) 
                return BadRequest("Ví của người dùng không tồn tại");

            // Remove the wallet
            _context.Wallets.Remove(walletExist);
            await _context.SaveChangesAsync(); // Save changes to persist deletion
            return Ok("Ví đã được xóa thành công.");
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
            if (string.IsNullOrEmpty(notification.OrderId) || string.IsNullOrEmpty(notification.RequestId) || string.IsNullOrEmpty(notification.walletId))
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
                var existWallet = await _context.Wallets.FindAsync(request.walletId);
                if (existWallet == null)
                {
                    return BadRequest($"Không tìm thấy ví người dùng với ID: {request.walletId}.");
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
      [HttpPost("LinkBankAccountTingee")]
public async Task<IActionResult> LinkBankAccountTingee([FromBody] BankLinkAccountTingeeRequest tingeeRequest)
{
    if (!IsValidAccountType(tingeeRequest.AccountType))
    {
        return BadRequest($"{tingeeRequest.AccountType} không hợp lệ.");
    }
    if (!BankValidator.CheckValidBankSupportTingee(tingeeRequest.BankName))
    {
        return BadRequest($"{tingeeRequest.BankName} không phải là ngân hàng hợp lệ trong danh sách hỗ trợ của chúng tôi.");
    }
    var (statusCode, resultContent) = await _tingeeApi.CreateBankLinkAsync(
                tingeeRequest.AccountType,
                tingeeRequest.BankName,
                tingeeRequest.AccountNumber,
                tingeeRequest.AccountName,
                tingeeRequest.Identity,
                tingeeRequest.Mobile,
                tingeeRequest.Email
            );

            // Kiểm tra mã trạng thái
            if (statusCode == HttpStatusCode.OK) // Kiểm tra mã thành công
            {
                var result = JsonConvert.DeserializeObject<dynamic>(resultContent);

                // Kiểm tra giá trị của trường code
                if (result.code.ToString() == "00")
                {
                    // Thêm ví vào cơ sở dữ liệu
                    string bankCode = tingeeRequest.BankName;
                    var wallet = new Wallet
                    {
                        Id = Guid.NewGuid().ToString(),
                        CardNumber = tingeeRequest.AccountNumber,
                        IsAvailable = false,
                        Name = tingeeRequest.BankName,
                        Userid = tingeeRequest.UserId,
                        Type = tingeeRequest.RequestType,
                    };
                    ProcessBanks(wallet.Id, bankCode, out string walletImage);
                    wallet.WalletImage = walletImage;
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
                        walletImage = wallet.WalletImage,
                    };

                    return Ok(new
                    {
                        statusCode = 200,
                        success = true,
                        message = "Liên kết ngân hàng thành công.",
                        walletData = walletResponse,
                        resultContent
                    });
                }
                else
                {
                    // Nếu mã code không phải "00", trả về thông điệp lỗi từ phản hồi Tingee
                    var errorMessage = result.message.ToString() ?? "Có lỗi xảy ra trong quá trình liên kết.";
                    return BadRequest(errorMessage);
                }
            }
            else
            {
                // Nếu mã trạng thái không phải 200, trả về thông điệp lỗi từ phản hồi Tingee
                var errorMessage = JsonConvert.DeserializeObject<dynamic>(resultContent)?.message.ToString() ?? "Lỗi không xác định.";
                return StatusCode((int)statusCode, errorMessage); 
            }
}
private bool IsValidAccountType(string accountType)
{
    return accountType == "personal-account" || accountType == "business-account";
}

private void ProcessBanks(string walletId, string code, out string walletImage)
{
    walletImage = string.Empty; // Initialize the variable to store the image path

    // Dictionary to map bank codes to their respective image URLs
    var bankImages = new Dictionary<string, string>
    {
        { "MBB", "https://api.vietqr.io/img/MB.png" },
        { "ICB", "https://api.vietqr.io/img/ICB.png" },
        { "VCB", "https://api.vietqr.io/img/VCB.png" },
        { "BIDV", "https://api.vietqr.io/img/BIDV.png" },
        { "VBA", "https://api.vietqr.io/img/VBA.png" },
        { "OCB", "https://api.vietqr.io/img/OCB.png" },
        { "TCB", "https://api.vietqr.io/img/TCB.png" },
        { "ACB", "https://api.vietqr.io/img/ACB.png" },
        { "VPB", "https://api.vietqr.io/img/VPB.png" },
        { "TPB", "https://api.vietqr.io/img/TPB.png" },
        { "STB", "https://api.vietqr.io/img/STB.png" },
        { "HDB", "https://api.vietqr.io/img/HDB.png" },
        { "VCCB", "https://api.vietqr.io/img/VCCB.png" },
        { "SCB", "https://api.vietqr.io/img/SCB.png" },
        { "VIB", "https://api.vietqr.io/img/VIB.png" },
        { "SHB", "https://api.vietqr.io/img/SHB.png" },
        { "EIB", "https://api.vietqr.io/img/EIB.png" },
        { "MSB", "https://api.vietqr.io/img/MSB.png" },
        { "CAKE", "https://api.vietqr.io/img/CAKE.png" },
        { "Ubank", "https://api.vietqr.io/img/UBANK.png" },
        { "TIMO", "https://vietqr.net/portal-service/resources/icons/TIMO.png" },
        { "VTLMONEY", "https://api.vietqr.io/img/VIETTELMONEY.png" },
        { "VNPTMONEY", "https://api.vietqr.io/img/VNPTMONEY.png" },
        { "SGICB", "https://api.vietqr.io/img/SGICB.png" },
        { "BAB", "https://api.vietqr.io/img/BAB.png" },
        { "PVCB", "https://api.vietqr.io/img/PVCB.png" },
        { "Oceanbank", "https://api.vietqr.io/img/OCEANBANK.png" },
        { "PGB", "https://api.vietqr.io/img/PGB.png" }, // PGBank
        { "VIETBANK", "https://api.vietqr.io/img/VIETBANK.png" }, // VietBank
        { "BVB", "https://api.vietqr.io/img/BVB.png" }, // BaoVietBank
        { "SEAB", "https://api.vietqr.io/img/SEAB.png" }, // SeABank
        { "COOPBANK", "https://api.vietqr.io/img/COOPBANK.png" }, // COOPBANK
        { "LPB", "https://api.vietqr.io/img/LPB.png" }, // LPBank
        { "KLB", "https://api.vietqr.io/img/KLB.png" }, // KienLongBank
        { "KBank", "https://api.vietqr.io/img/KBANK.png" }, // KBank
        { "KBHN", "https://api.vietqr.io/img/KBHN.png" }, // KookminHN
        { "KEBHANAHCM", "https://api.vietqr.io/img/KEBHANAHCM.png" }, // KEBHanaHCM
        { "KEBHANAHN", "https://api.vietqr.io/img/KEBHANAHN.png" }, // KEBHANAHN
        { "MAFC", "https://api.vietqr.io/img/MAFC.png" }, // MAFC
        { "CITIBANK", "https://api.vietqr.io/img/CITIBANK.png" }, // Citibank
        { "KBHCM", "https://api.vietqr.io/img/KBHCM.png" }, // KookminHCM
        { "VBSP", "https://api.vietqr.io/img/VBSP.png" }, // VBSP
        { "WVN", "https://api.vietqr.io/img/WVN.png" }, // Woori
        { "VRB", "https://api.vietqr.io/img/VRB.png" }, // VRB
        { "UOB", "https://api.vietqr.io/img/UOB.png" }, // UnitedOverseas
        { "SCVN", "https://api.vietqr.io/img/SCVN.png" }, // StandardChartered
        { "PBVN", "https://api.vietqr.io/img/PBVN.png" }, // PublicBank
        { "NHB HN", "https://api.vietqr.io/img/NHB.png" }, // Nonghyup
        { "IVB", "https://api.vietqr.io/img/IVB.png" }, // IndovinaBank
        { "IBK - HCM", "https://api.vietqr.io/img/IBK.png" }, // IBKHCM
        { "IBK - HN", "https://api.vietqr.io/img/IBK.png" }, // IBKHN
        { "HSBC", "https://api.vietqr.io/img/HSBC.png" }, // HSBC
        { "HLBVN", "https://api.vietqr.io/img/HLBVN.png" }, // HongLeong
        { "GPB", "https://api.vietqr.io/img/GPB.png" }, // GPBank
        { "DOB", "https://api.vietqr.io/img/DOB.png" }, // DongABank
        { "DBS", "https://api.vietqr.io/img/DBS.png" }, // DBSBank
        { "CIMB", "https://api.vietqr.io/img/CIMB.png" }, // CIMB
        { "CBB", "https://api.vietqr.io/img/CBB.png" } // CBBank
        // Add other banks as needed
    };

    // Check if the bank code exists in the dictionary
    if (bankImages.TryGetValue(code, out string imageUrl))
    {
        walletImage = imageUrl; // Set the wallet image to the corresponding URL
    }
    else
    {
        // If no matching bank code is found, set a default image URL
        walletImage = "https://default-image-url.com/default.png";
    }
}


        [HttpPost("ConfirmBankLinkTingee")]
        public async Task<IActionResult> ConfirmBankLinkTingee([FromBody] ConfirmBankLinkRequest request)
        {
            // Kiểm tra thông tin xác thực OTP
            var (code, message) = await _tingeeApi.ConfirmBankLinkAsync(request.BankName, request.ConfirmId, request.OtpNumber);

            if (code == "00")
            {
                // Nếu xác thực thành công, lưu thông tin ví
                var walletId = request.WalletId; // Sử dụng walletId từ request hoặc từ một nguồn khác

                var (success, saveMessage, wallet) = await SaveWallet(walletId);

                if (success)
                {
                    // Nếu lưu thành công, trả về thông điệp thành công
                    return Ok(new
                    {
                        message = $"Xác thực thành công. {saveMessage}",
                        statusCode = 200 ,
                        wallet
                    });
                }
                else
                {
                    // Nếu lưu không thành công, trả về thông điệp lỗi
                    return BadRequest(saveMessage);
                }
            }
            else
            {
                // Nếu xác thực không thành công, trả về thông báo lỗi từ phản hồi
                return BadRequest(message.ToString() ?? "Có lỗi xảy ra trong quá trình xác thực.");
            }
        }

    }
}
