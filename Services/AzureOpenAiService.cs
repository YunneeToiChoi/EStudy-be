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
            var requestBody = new
            {
                prompt = prompt,
                max_tokens = 150
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_endpoint}", content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);
            return jsonDoc.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();
        }
    }
}
