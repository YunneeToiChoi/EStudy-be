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
using Org.BouncyCastle.Ocsp;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.ValueContentAnalysis;

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

        [HttpPost("Get_AllListenChossenVocab")]
        public async Task<IActionResult> Get_AllListenChossenVocab([FromBody] VocabFlashCardRequest _vocabRequest)
        {
            if (_vocabRequest.lessonId == default(int))
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
                            .FirstOrDefaultAsync();

                var lessonTagResponse = new
                {
                    lessonTag = lessonTag?.TagId
                };

                var responseData = allVocabOfLesson.Select(vocab => new VocabListenChoosenResponse
                {
                    vocabId = vocab.VocabId,
                    vocabMean = vocab.Mean,
                    vocabTitle = vocab.VocabTitle,
                    vocabAudioUrl = vocab.AudioUrlUk
                }).ToList();

                const int chunkSize = 9;
                const int chunkLength = 20;
                var random = new Random();
                var chunkedData = new List<object>();

                for (int i = 0; i < responseData.Count; i += chunkSize)
                {
                    var chunk = responseData.Skip(i).Take(chunkSize).ToList();
                    while (chunk.Count < chunkSize)
                    {
                        chunk.Add(responseData[random.Next(responseData.Count)]);
                    }

                    var randomVocab = chunk[random.Next(chunk.Count)];

                    chunkedData.Add(new
                    {
                        url = randomVocab.vocabAudioUrl,
                        correct = randomVocab.vocabId,
                        listVocab = chunk
                    });
                }

                // Ensure there are at least 20 chunks
                while (chunkedData.Count < chunkLength)
                {
                    var newChunk = new List<VocabListenChoosenResponse>();
                    while (newChunk.Count < chunkSize)
                    {
                        newChunk.Add(responseData[random.Next(responseData.Count)]);
                    }
                    var randomVocab = newChunk[random.Next(newChunk.Count)];
                    chunkedData.Add(new
                    {
                        chunkName = $"chunk {chunkedData.Count + 1}",
                        url = randomVocab.vocabAudioUrl,
                        correct = randomVocab.vocabId,
                        listVocab = newChunk
                    });
                }

                // If there are more than 20 chunks, take only the first 20
                chunkedData = chunkedData.Take(chunkLength).ToList();

                return Ok(new
                {
                    status = 200,
                    message = "Get All Vocab Of Lesson Successful",
                    data = chunkedData,
                    lessonTag = lessonTagResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _vocabRequest.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }
        private static IEnumerable<IEnumerable<T>> Partition<T>(IEnumerable<T> source, int chunkSize)
        {
            int i = 0;
            while (source.Skip(i * chunkSize).Any())
            {
                yield return source.Skip(i * chunkSize).Take(chunkSize);
                i++;
            }
        }
        [HttpPost("Get_AllVocabFillWorld")]
        public async Task<IActionResult> Get_AllVocabFillWorld ([FromBody] OfLessonRequest _req)
        {
            if(_req.lessonId==0 || _req.lessonId == null)
            {
                _logger.LogWarning("LessonId is null or empty in the request.");
                return BadRequest(new { status = 400, message = "LessonId is null or empty" });
            }
            try
            {
                var vocab = await _context.Vocabularies.Where(v => v.LessonId == _req.lessonId).ToListAsync();
                var lessonTag = await _context.Lessons
                                             .Where(l => l.LessonId == _req.lessonId)
                                             .Select(l => l.Tag)
                                             .FirstAsync();
                var lessonTagResponse = new
                {
                    lessonTag = lessonTag.TagId
                };
                var fillWordResponse = vocab.Select(vocab => new VocabFillWorldResponse
                {
                    vocabId = vocab.VocabId,
                    vocabMean = vocab.Mean,
                    vocabTitle = vocab.VocabTitle,
                    vocabExplanation = vocab.Explanation,
                });
                return Json(new
                {
                    statusCode= 200,
                    messages = "Get All Vocab Fill World Successfull",
                    data = fillWordResponse,
                    lessonTag = lessonTagResponse,
                });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

    }
}
