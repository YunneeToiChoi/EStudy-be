using Polly;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NAudio.Wave;
using Polly.Extensions.Http;
using study4_be.Models;
using Microsoft.EntityFrameworkCore;

namespace study4_be.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpeakingController : ControllerBase
    {
        private readonly string _deploymentId1;
        private readonly string _deploymentId2;
        private readonly string _deploymentId3;
        private readonly string _deploymentId4;
        private readonly string _endpoint;
        private readonly string _speechKey;
        private readonly string _apiKey;
        private readonly string _region;
        private readonly Study4Context _context;

        public SpeakingController(IConfiguration configuration, Study4Context context)
        {
            _endpoint = configuration["AzureSpeech:Endpoint"];
            _speechKey = configuration["AzureSpeech:SubcriptionSpeechKey"];
            _apiKey = configuration["AzureSpeech:SubcriptionApiKey"];
            _deploymentId1 = configuration["AzureSpeech:DeploymentId1"];
            _deploymentId2 = configuration["AzureSpeech:DeploymentId2"];
            _deploymentId3 = configuration["AzureSpeech:DeploymentId3"];
            _deploymentId4 = configuration["AzureSpeech:DeploymentId4"];
            _region = configuration["AzureSpeech:Region"];
            _context = context;
        }

        [HttpPost("EvaluateQuestionBatch")]
        public async Task<IActionResult> EvaluateQuestionBatch([FromForm]List<IFormFile> audioFiles,[FromForm] List<int> questionIds, [FromForm]string userExamId)
        {
            if (audioFiles == null || audioFiles.Count == 0 || questionIds == null || questionIds.Count == 0 || audioFiles.Count != questionIds.Count)
            {
                return BadRequest("Invalid audio files or question IDs.");
            }

            var results = new List<dynamic>(); // To hold results for each question

            for (int i = 0; i < audioFiles.Count; i++)
            {
                var audioFile = audioFiles[i];
                var questionId = questionIds[i];

                // Save and process each audio file as before
                string tempFilePath = Path.GetTempFileName();
                string wavFilePath = tempFilePath + ".wav";

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(stream);
                }

                ConvertToWav(tempFilePath, wavFilePath);

                var speechConfig = SpeechConfig.FromSubscription(_speechKey, _region);
                var audioConfig = AudioConfig.FromWavFileInput(wavFilePath);

                try
                {
                    using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                    {
                        var result = await recognizer.RecognizeOnceAsync();
                        if (result.Reason == ResultReason.RecognizedSpeech)
                        {
                            string recognizedText = result.Text;
                            string topic = await GetTopicById(questionId);
                            if (topic == null)
                            {
                                return NotFound($"Topic with ID {questionId} not found.");
                            }

                            int modelNumber = i % 4 + 1;
                            string aiResponseJson = await ProcessWithAI(recognizedText, topic, modelNumber);
                            var aiResponseObject = System.Text.Json.JsonDocument.Parse(aiResponseJson);
                            string aiContent = aiResponseObject.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

                            // Define markers
                            string scoreMarker = "[SCORE]:";
                            string feedbackMarker1 = "[Feedback]:";
                            string feedbackMarker2 = "[FeedBack]:"; // Alternate handling

                            string score = "";
                            string feedback = "";

                            // Locate marker indices
                            int scoreIndex = aiContent.IndexOf(scoreMarker);
                            int feedbackIndex = aiContent.IndexOf(feedbackMarker1);
                            if (feedbackIndex == -1) feedbackIndex = aiContent.IndexOf(feedbackMarker2); // Fallback for alternate marker

                            // Extract score and feedback values if markers are found
                            if (scoreIndex != -1 && feedbackIndex > scoreIndex)
                            {
                                score = aiContent.Substring(scoreIndex + scoreMarker.Length, feedbackIndex - scoreIndex - scoreMarker.Length).Trim();
                            }
                            if (feedbackIndex != -1)
                            {
                                feedback = aiContent.Substring(feedbackIndex + (aiContent.Contains(feedbackMarker1) ? feedbackMarker1.Length : feedbackMarker2.Length)).Trim();
                            }

                            // Add structured result
                            results.Add(new
                            {
                                statusCode = 200,
                                questionId,
                                RecognizedText = recognizedText,
                                AiResponse = new
                                {
                                    Content = aiContent,
                                    Score = score,
                                    Feedback = feedback
                                }
                            });
                        }
                        else
                        {
                            results.Add(new
                            {
                                statusCode = 200,
                                questionId,
                                RecognizedText = "",
                                AiResponse = new
                                {
                                    Score = "0",
                                    Feedback = "No speech recognized."
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
                finally
                {
                    // Clean up temporary files if they exist
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }
            }

            var userExam = await _context.UsersExams.FindAsync(userExamId);
            if (userExam == null)
            {
                return NotFound($"User exam not found.");
            }
            // Tính tổng điểm từ results
            int totalScore = results.Sum(x => 
            {
                // Đảm bảo rằng x.AiResponse.Score có thể chuyển đổi thành int
                if (int.TryParse(x.AiResponse.Score?.ToString(), out int score)) 
                {
                    return score;
                }
                return 0; // Trả về 0 nếu không thể chuyển đổi
            });

            userExam.SpeakingScore += totalScore;
            // Cập nhật lại SpeakingScore trong userExam và session
            await _context.SaveChangesAsync();
            return Ok(results);
        }

        [HttpPost("EvaluateQuestionSix/{questionId:int}")]
        public async Task<IActionResult> EvaluateQuestionSix(IFormFile audioFile, int questionId)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }

            string tempFilePath = Path.GetTempFileName();
            string wavFilePath = tempFilePath + ".wav";

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            ConvertToWav(tempFilePath, wavFilePath);

            var speechConfig = SpeechConfig.FromSubscription(_speechKey, _region);
            var audioConfig = AudioConfig.FromWavFileInput(wavFilePath);

            try
            {
                using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                {
                    var result = await recognizer.RecognizeOnceAsync();
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        string recognizedText = result.Text;
                        string topic = await GetTopicById(questionId);
                        if (topic == null)
                        {
                            return NotFound($"Topic with ID {questionId} not found.");
                        }

                        // Call AI and get the raw JSON response
                        string aiResponseJson = await ProcessWithAIFollowUp(recognizedText, topic);

                        var aiResponseObject = System.Text.Json.JsonDocument.Parse(aiResponseJson);
                        string aiContent = aiResponseObject.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        // Define markers, allowing for slight variations
                        string scoreMarker = "[SCORE]:";
                        string feedbackMarker1 = "[Feedback]:";
                        string feedbackMarker2 = "[FeedBack]:"; // Handle both versions
                        string followUpMarker = "[FollowUp]:";

                        string score = "";
                        string feedback = "";
                        string followUpQuestion = "";

                        // Find the start index of each marker
                        int scoreIndex = aiContent.IndexOf(scoreMarker);
                        int feedbackIndex = aiContent.IndexOf(feedbackMarker1);
                        if (feedbackIndex == -1) feedbackIndex = aiContent.IndexOf(feedbackMarker2); // Fallback for alternative spelling
                        int followUpIndex = aiContent.IndexOf(followUpMarker);

                        // Extract the score value
                        if (scoreIndex != -1 && feedbackIndex > scoreIndex)
                        {
                            score = aiContent.Substring(scoreIndex + scoreMarker.Length, feedbackIndex - scoreIndex - scoreMarker.Length).Trim();
                        }

                        // Extract the feedback value
                        if (feedbackIndex != -1)
                        {
                            if (followUpIndex > feedbackIndex)
                            {
                                feedback = aiContent.Substring(feedbackIndex + feedbackMarker1.Length, followUpIndex - feedbackIndex - feedbackMarker1.Length).Trim();
                            }
                            else
                            {
                                feedback = aiContent.Substring(feedbackIndex + feedbackMarker1.Length).Trim();
                            }
                        }

                        // Extract the follow-up question
                        if (followUpIndex != -1)
                        {
                            followUpQuestion = aiContent.Substring(followUpIndex + followUpMarker.Length).Trim();
                        }
                        var synthesizer = new SpeechSynthesizer(speechConfig, AudioConfig.FromDefaultSpeakerOutput());
                        await synthesizer.SpeakTextAsync(followUpQuestion);

                        return Ok(new
                        {
                            statusCode = 200,
                            recognizedText,
                            aiResponse = new
                            {
                                Score = score,
                                Feedback = feedback,
                                FollowUpQuestion = followUpQuestion
                            }
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            statusCode = 200,
                            aiResponse = new
                            {
                                Score = "0",
                                Feedback = "No speech recognized.",
                                FollowUpQuestion = ""
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            finally
            {
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



        // Helper methods for extracting score and follow-up question
        private string ExtractScoreFromResponse(string aiContent)
        {
            string scoreMarker = "[SCORE]:";
            int scoreIndex = aiContent.IndexOf(scoreMarker);
            if (scoreIndex != -1)
            {
                int scoreStart = scoreIndex + scoreMarker.Length;
                int scoreEnd = aiContent.IndexOf("\n", scoreStart);
                return aiContent.Substring(scoreStart, scoreEnd - scoreStart).Trim();
            }
            return "0";
        }

        private string ExtractFollowUpQuestionFromResponse(string aiContent)
        {
            string followUpMarker = "[FollowUp]:";
            int followUpIndex = aiContent.IndexOf(followUpMarker);
            if (followUpIndex != -1)
            {
                return aiContent.Substring(followUpIndex + followUpMarker.Length).Trim();
            }
            return "No follow-up question provided.";
        }


        [HttpPost("EvaluateQuestionFollowUp")]
        public async Task<IActionResult> EvaluateQuestion([FromForm] IFormFile audioFile, [FromForm] string followUpQuestion, [FromForm]  string userExamId)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("No audio file uploaded.");
            }
            

            string tempFilePath = Path.GetTempFileName();
            string wavFilePath = tempFilePath + ".wav";

            // Save the file to the temporary path
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Convert file to WAV
            ConvertToWav(tempFilePath, wavFilePath);

            var speechConfig = SpeechConfig.FromSubscription(_speechKey, _region);
            var audioConfig = AudioConfig.FromWavFileInput(wavFilePath);

            try
            {
                using (var recognizer = new SpeechRecognizer(speechConfig, audioConfig))
                {
                    var result = await recognizer.RecognizeOnceAsync();
                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        string recognizedText = result.Text;
                        // Call AI and get the raw JSON response
                        string aiResponseJson = await ProcessWithAI(recognizedText, followUpQuestion, 2);

                        // Parse AI response JSON to extract the relevant message content
                        var aiResponseObject = System.Text.Json.JsonDocument.Parse(aiResponseJson);
                        string aiContent = aiResponseObject.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        // Define markers
                        string scoreMarker = "[SCORE]:";
                        string feedbackMarker1 = "[Feedback]:";
                        string feedbackMarker2 = "[FeedBack]:"; // Alternate handling

                        string score = "";
                        string feedback = "";

                        // Locate marker indices
                        int scoreIndex = aiContent.IndexOf(scoreMarker);
                        int feedbackIndex = aiContent.IndexOf(feedbackMarker1);
                        if (feedbackIndex == -1) feedbackIndex = aiContent.IndexOf(feedbackMarker2); // Fallback for alternate marker

                        // Extract score and feedback values if markers are found
                        if (scoreIndex != -1 && feedbackIndex > scoreIndex)
                        {
                            score = aiContent.Substring(scoreIndex + scoreMarker.Length, feedbackIndex - scoreIndex - scoreMarker.Length).Trim();
                        }
                        if (feedbackIndex != -1)
                        {
                            feedback = aiContent.Substring(feedbackIndex + (aiContent.Contains(feedbackMarker1) ? feedbackMarker1.Length : feedbackMarker2.Length)).Trim();
                        }


                        var userExam = await _context.UsersExams.FindAsync(userExamId);
                        if (userExam == null)
                        {
                            return NotFound("User exam not found.");
                        }
                        int totalScore = int.Parse(score);
                        
                        int speakingScore = HttpContext.Session.GetInt32("SpeakingScore") ?? 0;

                        totalScore += speakingScore;
                        userExam.SpeakingScore = totalScore;

                        var newQuestion = new Question()
                        {
                            QuestionText = followUpQuestion,
                            ExamId = userExam.ExamId,
                        };
                        await _context.Questions.AddAsync(newQuestion);
                        await _context.SaveChangesAsync();
                        
                        var questionId = newQuestion.QuestionId;

                        var newUserAnswer = new UserAnswer()
                        {
                            QuestionId = questionId,
                            UserExamId = userExamId,
                            Answer = recognizedText,
                            Comment = feedback
                        };
                        await _context.UserAnswers.AddAsync(newUserAnswer);
                        await _context.SaveChangesAsync();
                        // Return the response in the required format
                        return Ok(new
                        {
                            statusCode = 200,
                            recognizedText,
                            aiResponse = new
                            {
                                Score = score,
                                Feedback = feedback
                            }
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            statusCode = 200,
                            aiResponse = new
                            {
                                Score = "0",
                                Feedback = "No speech recognized.No speech recognized."
                            }
                        });
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

        private string EnsureWavFormat(IFormFile audioFile)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav");

            using (var reader = new WaveFileReader(audioFile.OpenReadStream()))
            {
                // Ensure the format is 16-bit PCM, 16kHz or 44.1kHz
                var format = new WaveFormat(16000, 16, 1);
                using (var converted = new WaveFormatConversionStream(format, reader))
                {
                    WaveFileWriter.CreateWaveFile(tempFilePath, converted);
                }
            }
            return tempFilePath;
        }

        private async Task<string> ProcessWithAI(string inputText, string topic, int modelNumber)
        {
            // Cấu hình retry policy

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            string apiUrl;
            switch (modelNumber)
            {
                case 1:
                    apiUrl = $"{_endpoint}/openai/deployments/{_deploymentId1}/chat/completions?api-version=2024-08-01-preview";
                    break;
                case 2:
                    apiUrl = $"{_endpoint}/openai/deployments/{_deploymentId2}/chat/completions?api-version=2024-08-01-preview";
                    break;
                case 3:
                    apiUrl = $"{_endpoint}/openai/deployments/{_deploymentId3}/chat/completions?api-version=2024-08-01-preview";
                    break;
                case 4:
                    apiUrl = $"{_endpoint}/openai/deployments/{_deploymentId4}/chat/completions?api-version=2024-08-01-preview";
                    break;
                default:
                    throw new ArgumentException("Invalid model number");
            }
            
            using (var httpClient = new HttpClient())
            {
                int minScore = 0;
                int maxScore = 25;
                httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string prompt = $"The topic is '{topic}'. The user said: '{inputText}'. Suppose you are an exam examiner .Now, respond with feedback and score. [SCORE] : Value {minScore} -> {maxScore}, [FeedBack] : Value";
                var requestBody = new
                {
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Thực hiện yêu cầu với policy retry
                var response = await retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsync(apiUrl, content)
                );

                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : $"Error: {await response.Content.ReadAsStringAsync()}";
            }
        }
        private async Task<string> ProcessWithAIFollowUp(string inputText, string topic)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            using (var httpClient = new HttpClient())
            {
                int minScore = 0;
                int maxScore = 50;
                httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string prompt = $@"
                        The topic is '{topic}'. The user said: '{inputText}'.
                        Suppose you are an exam examiner evaluating a spoken response. 
                        1. Provide a score out of 5 based on factors like pronunciation, fluency, coherence, and relevance.
                        2. Give feedback on the response, noting strengths and areas for improvement.
                        3. Formulate a follow-up question to encourage the user to elaborate further on their answer or clarify details.
                        Respond with:
                        [SCORE]: Value {minScore} -> {maxScore}
                        [FeedBack]: Value
                        [FollowUp]: Value";

                var requestBody = new
                {
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsync($"{_endpoint}/openai/deployments/{_deploymentId1}/chat/completions?api-version=2024-08-01-preview", content)
                );
                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : $"Error: {await response.Content.ReadAsStringAsync()}";
            }
        }
        private async Task<string> GetTopicById(int questionId)
        {
            var topic = await _context.Questions.Where(q=> q.QuestionId == questionId).Select(e=>e.QuestionParagraph).FirstOrDefaultAsync();
            return topic;
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
    }
}
