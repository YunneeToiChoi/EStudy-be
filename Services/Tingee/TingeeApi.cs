using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer; // Thay đổi sang System.Text.Json nếu có thể

namespace study4_be.Services.Tingee;

public class TingeeApi
{
   private readonly string _clientId;
    private readonly string _secretToken;
    private readonly string _baseUrl;
    private readonly string _confirmUrl;
    private readonly string _merchantName; 
    private readonly string _merchantAddress; 

    public TingeeApi(IConfiguration configuration)
    {
        _clientId = configuration["Tingee:ClientId"];
        _secretToken = configuration["Tingee:SecretToken"];
        _baseUrl = configuration["Tingee:ApiUrl:Live"]; // Chọn giữa Live hoặc Test
        _confirmUrl = configuration["Tingee:ApiUrl:Confirm"];
        _merchantAddress = configuration["Tingee:ApiUrl:MerchantAddress"];
        _merchantName = configuration["Tingee:ApiUrl:MerchantName"]; 
    }

    public async Task<(HttpStatusCode StatusCode, string Message)> CreateBankLinkAsync(
      string accountType,
      string bankName,
      string accountNumber,
      string accountName,
      string identity,
      string mobile,
      string email)
    {
        using (var httpClient = new HttpClient())
        {
            // Thiết lập timeout
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Tạo dấu thời gian
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssSSS");

            // Tạo request body
            var requestBody = new
            {
                accountType,
                bankName,
                accountNumber,
                accountName,
                identity,
                mobile,
                email,
                merchantName = _merchantName,
                merchantAddress = _merchantAddress,
            };

            // Chuyển đổi request body thành chuỗi JSON
            string requestBodyJson = JsonSerializer.Serialize(requestBody);

            // Tạo chuỗi để hash
            string hashString = $"{timestamp}:{requestBodyJson}";

            // Tính toán x-signature
            string signature = ComputeHmacSha512(hashString, _secretToken);

            // Thiết lập headers
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("x-request-timestamp", timestamp);
            httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
            httpClient.DefaultRequestHeaders.Add("x-signature", signature);

            try
            {
                // Gửi yêu cầu POST
                var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_baseUrl, content);

                // Đọc phản hồi
                var responseContent = await response.Content.ReadAsStringAsync();

                // Trả về mã trạng thái và nội dung phản hồi
                return (response.StatusCode, responseContent);
            }
            catch (HttpRequestException ex)
            {
                // Xử lý lỗi khi gửi yêu cầu HTTP
                return (HttpStatusCode.InternalServerError, $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                return (HttpStatusCode.InternalServerError, $"Lỗi không xác định: {ex.Message}");
            }
        }
    }


    public async Task<(string Code, string Message)> ConfirmBankLinkAsync(string bankName, string confirmId, string otpNumber)
    {
        using (var httpClient = new HttpClient())
        {
            // Tạo dấu thời gian
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssSSS");

            // Tạo request body
            var requestBody = new
            {
                bankName = bankName,
                confirmId = confirmId,
                otpNumber = otpNumber
            };

            // Chuyển đổi request body thành chuỗi JSON
            string requestBodyJson = JsonConvert.SerializeObject(requestBody);

            // Tạo chuỗi để hash
            string hashString = $"{timestamp}:{requestBodyJson}";

            // Tính toán x-signature
            string signature = ComputeHmacSha512(hashString, _secretToken);

            // Thiết lập headers
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("x-request-timestamp", timestamp);
            httpClient.DefaultRequestHeaders.Add("x-client-id", _clientId);
            httpClient.DefaultRequestHeaders.Add("x-signature", signature);

            // Gửi yêu cầu POST
            var content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_confirmUrl, content); 

            // Đọc phản hồi
            var responseContent = await response.Content.ReadAsStringAsync();

            // Kiểm tra mã trạng thái
            if (!response.IsSuccessStatusCode)
            {
                // Nếu không thành công, trả về mã và thông điệp lỗi
                return ("Error", $"Error: {response.StatusCode}, Message: {responseContent}");
            }

            // Phân tích phản hồi JSON để lấy mã và thông điệp
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            return (result.code.ToString(), result.message.ToString());
        }
    }

    private string ComputeHmacSha512(string data, string key)
    {
        using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    } 
}