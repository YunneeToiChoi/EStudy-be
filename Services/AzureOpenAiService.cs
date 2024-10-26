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

        public AzureOpenAiService(IConfiguration configuration)
        {
            _endpoint = configuration["AzureOpenAI:Endpoint"];
            _apiKey = configuration["AzureOpenAI:ApiKey"];
            _deploymentId = configuration["AzureOpenAI:DeploymentId"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            Console.WriteLine(prompt);
            var requestBody = new
            {
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 150,
                temperature = 0.7 // Bạn có thể điều chỉnh giá trị này theo nhu cầu
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_endpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version=2024-08-01-preview", content);


            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}