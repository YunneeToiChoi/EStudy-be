using System.Text.Json;
using System.Text;


namespace study4_be.Services
{
    public class AzureOpenAiService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _deploymentId;
        private readonly HttpClient _httpClient;
        private int _tokenCount = 0;
        private DateTime _lastResetTime = DateTime.UtcNow;
        public AzureOpenAiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _endpoint = configuration["AzureOpenAI:Endpoint"];
            _apiKey = configuration["AzureOpenAI:ApiKey"];
            _deploymentId = configuration["AzureOpenAI:DeploymentId"];
        
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }
        private int EstimateTokens(string text)
        {
            // Tính toán số ký tự trong văn bản
            int characterCount = text.Length;

            // Ước lượng số token bằng cách chia số ký tự cho 4
            // Cộng thêm một để đảm bảo không bị thiếu nếu không chia hết
            int tokenCount = (int)Math.Ceiling(characterCount / 4.0);
    
            return tokenCount;
        }
        
        private void ResetTokenCount()
        {
            if ((DateTime.UtcNow - _lastResetTime).TotalMinutes > 1)
            {
                _tokenCount = 0; // Đặt lại số token đã sử dụng
                _lastResetTime = DateTime.UtcNow; // Cập nhật thời gian đặt lại
            }
        }
        
        public async Task<string> GenerateResponseAsync(string prompt, int maxTokens = 100, double temperature = 0.7)
        {
            ResetTokenCount(); // Kiểm tra và đặt lại số token nếu cần

            // Tính toán số token cho prompt và phản hồi
            int promptTokenCount = EstimateTokens(prompt);
            int totalTokenCount = promptTokenCount + maxTokens;

            // Nếu số token đã sử dụng cộng với số token yêu cầu vượt quá giới hạn, hãy đợi
            if (_tokenCount + totalTokenCount > 1000)
            {
                // Tính toán thời gian cần đợi
                await Task.Delay(TimeSpan.FromMinutes(1)); // Đợi 1 phút
                ResetTokenCount(); // Đặt lại số token đã sử dụng
            }

            // Tiến hành gửi yêu cầu
            var requestBody = new
            {
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = maxTokens,
                temperature = temperature
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_endpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version=2024-08-01-preview", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);

            // Cập nhật số token đã sử dụng
            _tokenCount += promptTokenCount + maxTokens;

            return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

    }
}