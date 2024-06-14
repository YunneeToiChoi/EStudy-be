using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request;
using study4_be.Services.Response;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Google.Cloud.Storage.V1;
using study4_be.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class VocabFlashCard_APIController : Controller
    {
        private readonly STUDY4Context _context;
        private readonly VocabFlashCardRepository _vocabFlashCardRepo;
        private readonly ILogger<VocabFlashCard_APIController> _logger;
        private readonly FireBaseServices _firebaseServices;
        public VocabFlashCard_APIController(ILogger<VocabFlashCard_APIController> logger, FireBaseServices firebaseServices)
        {
            _context = new STUDY4Context();
            _vocabFlashCardRepo = new VocabFlashCardRepository();
            _logger = logger;
            _firebaseServices = firebaseServices;
        }
        [HttpPost("Get_AllVocabOfLesson")]
        public async Task<IActionResult> Get_AllVocabOfLesson([FromBody] VocabFlashCardRequest _vocabRequest) {
            if (_vocabRequest.lessonId == null)
            {
                _logger.LogWarning("LessonId is null or empty in the request.");
                return BadRequest(new { status = 400, message = "LessonId is null or empty" });
            }

            try
            {
                var allVocabOfLesson = await _vocabFlashCardRepo.GetAllVocabDependLesson(_vocabRequest.lessonId);
                var lessonTag = await _context.Lessons
                                           .Where(l => l.LessonId == _vocabRequest.lessonId)
                                           .Select(l => l.Tag)
                                           .FirstAsync();
                var lessonTagResponse = new
                {
                    lessonTag = lessonTag.TagId
                };
                return Ok(new { status = 200, message = "Get All Vocab Of Lesson Successful", data = allVocabOfLesson, lessonTag = lessonTagResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _vocabRequest.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("Get_AllVocabFindpair")]
        public async Task<IActionResult> Get_AllVocabFindpair([FromBody] VocabFlashCardRequest _vocabRequest)
        {
            if (_vocabRequest.lessonId == default(int))
            {
                _logger.LogWarning("LessonId is null or empty in the request.");
                return BadRequest(new { status = 400, message = "LessonId is null or empty" });
            }

            const int chunkSize = 8;
            const int chunkLength = 20;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var vocabTask = _vocabFlashCardRepo.GetAllVocabDependLesson(_vocabRequest.lessonId);
                var tagTask = _context.Lessons
                                      .Where(l => l.LessonId == _vocabRequest.lessonId)
                                      .Select(l => l.Tag)
                                      .FirstOrDefaultAsync();

                await Task.WhenAll(vocabTask, tagTask);

                var allVocabOfLesson = vocabTask.Result;
                var lessonTag = tagTask.Result?.TagId;

                var responseData = allVocabOfLesson.Select(vocab => new VocabFindPairResponse
                {
                    vocabId = vocab.VocabId,
                    vocabMean = vocab.Mean,
                    vocabExplanation = vocab.Explanation,
                    vocabTitle = vocab.VocabTitle
                }).ToList();

                var chunkedLists = new List<List<VocabFindPairResponse>>();
                var random = new Random();

                for (int i = 0; i < responseData.Count; i += chunkSize)
                {
                    var chunk = responseData.Skip(i).Take(chunkSize).ToList();
                    while (chunk.Count < chunkSize)
                    {
                        chunk.Add(responseData[random.Next(responseData.Count)]);
                    }
                    chunkedLists.Add(chunk);
                }

                while (chunkedLists.Count < chunkLength)
                {
                    var randomChunk = chunkedLists[random.Next(chunkedLists.Count)];
                    var newChunk = new List<VocabFindPairResponse>(randomChunk);
                    while (newChunk.Count < chunkSize)
                    {
                        newChunk.Add(responseData[random.Next(responseData.Count)]);
                    }
                    chunkedLists.Add(newChunk);
                }

                var chunkedData = chunkedLists.Take(chunkLength).Select((chunk, index) => new
                {
                    chunkName = $"chunk{index + 1}",
                    vocabulary = chunk
                }).ToList();

                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed.TotalSeconds;

                return Ok(new
                {
                    status = 200,
                    message = "Get All Vocab Of Lesson Successful",
                    data = chunkedData,
                    lessonTag = new { lessonTag },
                    elapsedTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _vocabRequest.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }

        // Helper method to partition list into chunks
        private static IEnumerable<IEnumerable<T>> Partition<T>(IEnumerable<T> source, int chunkSize)
        {
            int i = 0;
            while (source.Skip(i * chunkSize).Any())
            {
                yield return source.Skip(i * chunkSize).Take(chunkSize);
                i++;
            }
        }

         [HttpPost("Get_AllListenChossenVocab")]
        public async Task<IActionResult> Get_AllListenChossenVocab([FromBody] VocabFlashCardRequest _vocabRequest)
        {
            try
            {
                var allVocabOfLesson = await _vocabFlashCardRepo.GetAllVocabDependLesson(_vocabRequest.lessonId);

                var firebaseBucketName =  _firebaseServices.GetFirebaseBucketName();

                // Use firebaseBucketName as needed...
                var lessonTag = await _context.Lessons
                         .Where(l => l.LessonId == _vocabRequest.lessonId)
                         .Select(l => l.Tag)
                         .FirstAsync();
                var lessonTagResponse = new
                {
                    lessonTag = lessonTag.TagId
                };
                var responseData = new List<VocabListenChoosenResponse>();
                foreach (var vocab in allVocabOfLesson)
                {
                    // Generate and upload audio to Firebase Storage
                    var audioFilePath = Path.Combine(Path.GetTempPath(), $"{vocab.VocabId}.wav");
                    GenerateAudio(vocab.VocabTitle, audioFilePath);

                    var audioBytes = System.IO.File.ReadAllBytes(audioFilePath);
                    var audioUrl = await UploadFileToFirebaseStorageAsync(audioBytes, $"{vocab.VocabId}.wav", firebaseBucketName);
                    // Example: Upload to Firebase Storage using firebaseBucketName...

                    responseData.Add(new VocabListenChoosenResponse
                    {
                        vocabId = vocab.VocabId,
                        vocabMean = vocab.Mean,
                        vocabTitle = vocab.VocabTitle,
                        vocabAudioUrl = audioUrl
                    });
                    // Delete the temporary file after uploading
                    System.IO.File.Delete(audioFilePath);
                }

                return Ok(new { status = 200, message = "Get All Vocab Of Lesson Successful", data = responseData, lessontag=lessonTagResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _vocabRequest.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }

        // Thực hiện tải tệp lên Firebase Storage
        public async Task<string> UploadFileToFirebaseStorageAsync(byte[] fileBytes, string fileName, string bucketName)
        {
            // Assuming your service account file is named "serviceAccount.json"
            string serviceAccountPath = Path.Combine(Directory.GetCurrentDirectory(), "firebase_config.json");

            // Load the credential from the file
            var credential = GoogleCredential.FromFile(serviceAccountPath);

            // Create a StorageClient object
            var storage = StorageClient.Create(credential);
                    string correctedBucketName = "estudy-426108.appspot.com"; // Assuming the correct name is 'estudy426108'
            // Create a MemoryStream object from the file bytes
            using (var memoryStream = new MemoryStream(fileBytes))
            {
                // Upload the file to Firebase Storage
                try
                {
                    var storageObject = await storage.UploadObjectAsync(correctedBucketName, fileName, null, memoryStream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading file: {ex.Message}");
                }
                return $"https://firebasestorage.googleapis.com/v0/b/{correctedBucketName}/o/{fileName}?alt=media";
            }
        }
        private void GenerateAudio(string text, string filePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Program Files (x86)\eSpeak NG\espeak-ng.exe",
                Arguments = $"-w \"{filePath}\" \"{text}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
            }
        }

    }
}
