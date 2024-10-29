using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore; // For database context
using System.Collections.Generic;
using study4_be.Models;
using System.Drawing;

namespace SpeechAI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeakingController : ControllerBase
    {
        private readonly string _deploymentId;
        private readonly string _endpoint;
        private readonly string _speechKey;
        private readonly string _apiKey;
        private readonly string _region;
        private readonly Study4Context _context; // Your Entity Framework DbContext

        public SpeakingController(IConfiguration configuration, Study4Context context)
        {
            _endpoint = configuration["AzureSpeech:Endpoint"];
            _speechKey = configuration["AzureSpeech:SubcriptionSpeechKey"];
            _apiKey = configuration["AzureSpeech:SubcriptionApiKey"];
            _deploymentId = configuration["AzureSpeech:DeploymentId"];
            _region = configuration["AzureSpeech:Region"];
            _context = context; // Initialize the DbContext
        }
        [HttpPost("start-conversation")]
        public async Task<IActionResult> StartConversation(IFormFile audioFile, [FromQuery] int questionId)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }

            string tempFilePath = Path.GetTempFileName(); // Create temporary file
            string wavFilePath = tempFilePath + ".wav"; // Temporary WAV file path

            // Save the file to the temporary path
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Convert file to WAV
            ConvertToWav(tempFilePath, wavFilePath);

            var speechConfig = SpeechConfig.FromSubscription(_speechKey, _region);
            var audioConfig = AudioConfig.FromWavFileInput(wavFilePath); // WAV file path

            try
            {
                using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                {
                    var result = await recognizer.RecognizeOnceAsync();
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        string recognizedText = result.Text;
                        string topic = await GetTopicById(questionId); // Fetch the topic from the database
                        if (topic == null)
                        {
                            return NotFound($"Topic with ID {questionId} not found.");
                        }

                        string aiResponse = await ProcessWithAI(recognizedText, topic); // Get AI response

                        // Speak the AI response
                         var synthesizer = new SpeechSynthesizer(speechConfig, AudioConfig.FromDefaultSpeakerOutput());
                        await synthesizer.SpeakTextAsync(aiResponse);

                        return Ok(new { recognizedText, aiResponse });
                    }
                    else
                    {
                        return BadRequest("Could not recognize speech.");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
                // Clean up temporary files
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
                if (System.IO.File.Exists(wavFilePath))
                {
                    System.IO.File.Delete(wavFilePath);
                }
            }
        }

        private void ConvertToWav(string inputFilePath, string outputFilePath)
        {
            using (var reader = new AudioFileReader(inputFilePath))
            {
                using (var writer = new WaveFileWriter(outputFilePath, reader.WaveFormat))
                {
                    reader.CopyTo(writer);
                }
            }
        }

        private async Task<string> ProcessWithAI(string inputText, string topic)
        {
            using (var httpClient = new HttpClient())
            {
                // Use api-key for Azure OpenAI instead of Bearer token
                httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Prepare the prompt by combining the topic and user input
                string prompt = $"The topic is '{topic}'. The user said: '{inputText}'. Now, respond as if you are having a conversation, and ask a follow-up question.";

                var requestBody = new
                {
                    model = _deploymentId,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Correct endpoint without double slashes
                var response = await httpClient.PostAsync($"{_endpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version=2024-08-01-preview", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var aiResponse = System.Text.Json.JsonDocument.Parse(jsonResponse)
                        .RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return aiResponse;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return $"Error calling GPT API: {errorResponse}";
                }
            }
        }
        private async Task<string> GetTopicById(int questionId)
        {
            // Query the database to get the topic based on the questionId
            var topic = await _context.Questions.FindAsync(questionId);
            return "Introduct your self ";
            //return topic?.QuestionParagraph; 
        }
    }
}
