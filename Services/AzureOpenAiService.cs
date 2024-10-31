using System.Text.Json;
using System.Text;
using Polly.Extensions.Http;
using Polly; 


namespace study4_be.Services
{
    public class AzureOpenAiService
    {
        private readonly string _endpointModel1;
        private readonly string _endpointModel2;
        private readonly string _apiKey1;
        private readonly string _apiKey2;
        private readonly string _deploymentIdModel1;
        private readonly string _deploymentIdModel2;
        private readonly string _deploymentIdModel3;
        private readonly string _deploymentIdModel4;
        private readonly object _resetLock = new object();
        private readonly HttpClient _httpClient;
        private int _tokenCountModel1 = 0;
        private int _tokenCountModel2 = 0;
        private int _tokenCountModel3 = 0;
        private int _tokenCountModel4 = 0;
        private DateTime _lastResetTimeModel1 = DateTime.UtcNow;
        private DateTime _lastResetTimeModel2 = DateTime.UtcNow;
        private DateTime _lastResetTimeModel3 = DateTime.UtcNow;
        private DateTime _lastResetTimeModel4 = DateTime.UtcNow;

        public AzureOpenAiService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _endpointModel1 = configuration["AzureOpenAI:EndpointModel1"];
            _endpointModel2 = configuration["AzureOpenAI:EndpointModel2"];
            _apiKey1 = configuration["AzureOpenAI:ApiKey1"];
            _apiKey2 = configuration["AzureOpenAI:ApiKey2"];
            _deploymentIdModel1 = configuration["AzureOpenAI:DeploymentIdModel1"];
            _deploymentIdModel2 = configuration["AzureOpenAI:DeploymentIdModel2"];
            _deploymentIdModel3 = configuration["AzureOpenAI:DeploymentIdModel3"];
            _deploymentIdModel4 = configuration["AzureOpenAI:DeploymentIdModel4"];

            _httpClient = httpClientFactory.CreateClient();
        }

        private int EstimateTokens(string text)
        {
            int characterCount = text.Length;
            int tokenCount = (int)Math.Ceiling(characterCount / 4.0);
            return tokenCount;
        }

        private void ResetTokenCount()
        {
            lock (_resetLock)
            {
                if ((DateTime.UtcNow - _lastResetTimeModel1).TotalMinutes >= 1)
                {
                    _tokenCountModel1 = 0;
                    _lastResetTimeModel1 = DateTime.UtcNow;
                }
                if ((DateTime.UtcNow - _lastResetTimeModel2).TotalMinutes >= 1)
                {
                    _tokenCountModel2 = 0;
                    _lastResetTimeModel2 = DateTime.UtcNow;
                }
                if ((DateTime.UtcNow - _lastResetTimeModel3).TotalMinutes >= 1)
                {
                    _tokenCountModel3 = 0;
                    _lastResetTimeModel3 = DateTime.UtcNow;
                }
                if ((DateTime.UtcNow - _lastResetTimeModel4).TotalMinutes >= 1)
                {
                    _tokenCountModel4 = 0;
                    _lastResetTimeModel4 = DateTime.UtcNow;
                }
            }
        }

        public async Task<string> GenerateResponseAsync(string prompt, int modelNumber, int maxTokens = 100, double temperature = 0.7)
        {
            ResetTokenCount();
            int promptTokenCount = EstimateTokens(prompt);
            int totalTokenCount = promptTokenCount + maxTokens;
            int currentTokenCount;

            // Chọn endpoint, API key và token count dựa trên model
            string apiUrl;
            string apiKey;

            switch (modelNumber)
            {
                case 1:
                    apiUrl = $"{_endpointModel1}/openai/deployments/{_deploymentIdModel1}/chat/completions?api-version=2024-08-01-preview";
                    apiKey = _apiKey1;
                    currentTokenCount = _tokenCountModel1;
                    break;
                case 2:
                    apiUrl = $"{_endpointModel1}/openai/deployments/{_deploymentIdModel2}/chat/completions?api-version=2024-08-01-preview";
                    apiKey = _apiKey1;
                    currentTokenCount = _tokenCountModel2;
                    break;
                case 3:
                    apiUrl = $"{_endpointModel1}/openai/deployments/{_deploymentIdModel3}/chat/completions?api-version=2024-08-01-preview";
                    apiKey = _apiKey1;
                    currentTokenCount = _tokenCountModel3;
                    break;
                case 4:
                    apiUrl = $"{_endpointModel2}/openai/deployments/{_deploymentIdModel4}/chat/completions?api-version=2024-08-01-preview";
                    apiKey = _apiKey2;
                    currentTokenCount = _tokenCountModel4;
                    break;
                default:
                    throw new ArgumentException("Invalid model number");
            }

            // Kiểm tra giới hạn token
            if (currentTokenCount + totalTokenCount >= 1000)
            {
                await Task.Delay(TimeSpan.FromMinutes(1)); // Đợi 1 phút nếu vượt quá giới hạn
                ResetTokenCount();
            }

            _httpClient.DefaultRequestHeaders.Remove("api-key");
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

            var requestBody = new
            {
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = maxTokens,
                temperature = temperature
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            // Policy retry với backoff
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            HttpResponseMessage response = await retryPolicy.ExecuteAsync(() => _httpClient.PostAsync(apiUrl, content));
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);

            // Cập nhật token count cho model
            switch (modelNumber)
            {
                case 1:
                    _tokenCountModel1 += totalTokenCount;
                    break;
                case 2:
                    _tokenCountModel2 += totalTokenCount;
                    break;
                case 3:
                    _tokenCountModel3 += totalTokenCount;
                    break;
                case 4:
                    _tokenCountModel4 += totalTokenCount;
                    break;
            }

            return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }

}