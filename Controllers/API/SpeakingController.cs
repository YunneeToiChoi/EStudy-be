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

namespace study4_be.Controllers.API
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
        private readonly Study4Context _context;

        public SpeakingController(IConfiguration configuration, Study4Context context)
        {
            _endpoint = configuration["AzureSpeech:Endpoint"];
            _speechKey = configuration["AzureSpeech:SubcriptionSpeechKey"];
            _apiKey = configuration["AzureSpeech:SubcriptionApiKey"];
            _deploymentId = configuration["AzureSpeech:DeploymentId"];
            _region = configuration["AzureSpeech:Region"];
            _context = context;
        }

        [HttpPost("EvaluateQuestionBatch")]
        public async Task<IActionResult> EvaluateQuestionBatch([FromForm]List<IFormFile> audioFiles,[FromForm] List<int> questionIds, string userExamId)
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

                            string aiResponseJson = await ProcessWithAI(recognizedText, topic);
                            var aiResponseObject = System.Text.Json.JsonDocument.Parse(aiResponseJson);
                            string aiContent = aiResponseObject.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString();

                            string scoreMarker = "[SCORE]:";
                            string feedbackMarker = "[FeedBack]:";
                            string score = "";
                            string feedback = "";

                            int scoreIndex = aiContent.IndexOf(scoreMarker);
                            if (scoreIndex != -1)
                            {
                                int feedbackIndex = aiContent.IndexOf(feedbackMarker, scoreIndex);
                                if (feedbackIndex != -1)
                                {
                                    score = aiContent.Substring(scoreIndex + scoreMarker.Length, feedbackIndex - scoreIndex - scoreMarker.Length).Trim();
                                    feedback = aiContent.Substring(feedbackIndex + feedbackMarker.Length).Trim();
                                }
                            }

                            results.Add(new
                            {
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
                    // Clean up temporary files
                    System.IO.File.Delete(tempFilePath);
                    System.IO.File.Delete(wavFilePath);
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
                // Đảm bảo x.Score là một string và không null
                if (int.TryParse(x.Score?.ToString(), out int score)) 
                {
                    return score;
                }
                return 0; // Nếu không thể chuyển đổi, trả về 0
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

                        // Extract [SCORE], [FeedBack], and follow-up question from the content
                        string scoreMarker = "[SCORE]:";
                        string feedbackMarker = "[FeedBack]:";
                        string followUpMarker = "[FollowUp]:";
                        string score = "";
                        string feedback = "";
                        string followUpQuestion = "";

                        // Lấy điểm
                        int scoreIndex = aiContent.IndexOf(scoreMarker);
                        if (scoreIndex != -1)
                        {
                            int scoreStart = scoreIndex + scoreMarker.Length;
                            int scoreEnd = aiContent.IndexOf("\n", scoreStart);
                            score = aiContent.Substring(scoreStart, scoreEnd - scoreStart).Trim();

                            // Lưu điểm vào session
                            HttpContext.Session.SetInt32("SpeakingScore", int.Parse(score));
                        }

                        // Lấy phản hồi
                        int feedbackIndex = aiContent.IndexOf(feedbackMarker);
                        if (feedbackIndex != -1)
                        {
                            int feedbackStart = feedbackIndex + feedbackMarker.Length;
                            int feedbackEnd = aiContent.IndexOf("\n", feedbackStart);
                            feedback = aiContent.Substring(feedbackStart, feedbackEnd - feedbackStart).Trim();
                        }

                        // Lấy câu hỏi tiếp theo
                        int followUpIndex = aiContent.IndexOf(followUpMarker);
                        if (followUpIndex != -1)
                        {
                            followUpQuestion = aiContent.Substring(followUpIndex + followUpMarker.Length).Trim();
                        }

                        var synthesizer = new SpeechSynthesizer(speechConfig, AudioConfig.FromDefaultSpeakerOutput());
                        await synthesizer.SpeakTextAsync(followUpQuestion);

                        return Ok(new
                        {
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
        public async Task<IActionResult> EvaluateQuestion(IFormFile audioFile, string followUpQuestion, string userExamId)
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
                        string aiResponseJson = await ProcessWithAI(recognizedText, followUpQuestion);

                        // Parse AI response JSON to extract the relevant message content
                        var aiResponseObject = System.Text.Json.JsonDocument.Parse(aiResponseJson);
                        string aiContent = aiResponseObject.RootElement
                                            .GetProperty("choices")[0]
                                            .GetProperty("message")
                                            .GetProperty("content")
                                            .GetString();

                        // Extract [SCORE] and [FeedBack] from the content
                        string scoreMarker = "[SCORE]:";
                        string feedbackMarker = "[FeedBack]:";
                        string score = "";
                        string feedback = "";

                        int scoreIndex = aiContent.IndexOf(scoreMarker);
                        if (scoreIndex != -1)
                        {
                            int scoreStart = scoreIndex + scoreMarker.Length;
                            int scoreEnd = aiContent.IndexOf("\n", scoreStart);
                            score = aiContent.Substring(scoreStart, scoreEnd - scoreStart).Trim();
                        }

                        int feedbackIndex = aiContent.IndexOf(feedbackMarker);
                        if (feedbackIndex != -1)
                        {
                            feedback = aiContent.Substring(feedbackIndex + feedbackMarker.Length).Trim();
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

        private async Task<string> ProcessWithAI(string inputText, string topic)
        {
            // Cấu hình retry policy

            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            using (var httpClient = new HttpClient())
            {
                int minScore = 0;
                int maxScore = 25;
                httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string prompt = $"The topic is '{topic}'. The user said: '{inputText}'. Suppose you are an exam examiner .Now, respond with feedback and score. [SCORE] : Value {minScore} -> {maxScore}, [FeedBack] : Value";
                var requestBody = new
                {
                    model = _deploymentId,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Thực hiện yêu cầu với policy retry
                var response = await retryPolicy.ExecuteAsync(() =>
                    httpClient.PostAsync($"{_endpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version=2024-08-01-preview", content)
                );

                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : $"Error: {await response.Content.ReadAsStringAsync()}";
            }
        }
        private async Task<string> ProcessWithAIFollowUp(string inputText, string topic)
        {
            using (var httpClient = new HttpClient())
            {
                
                var retryPolicy = HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                
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
                    model = _deploymentId,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync($"{_endpoint}/openai/deployments/{_deploymentId}/chat/completions?api-version=2024-08-01-preview", content);

                return response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync()
                    : $"Error: {await response.Content.ReadAsStringAsync()}";
            }
        }
        private async Task<string> GetTopicById(int questionId)
        {
            var topic = await _context.Questions.FindAsync(questionId);
            return topic?.QuestionParagraph ?? "What is accountant ? ";
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
