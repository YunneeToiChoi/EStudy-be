using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request;
using study4_be.Services.Response;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Question_APIController : Controller
    {
        public STUDY4Context _context;
        public QuestionRepository _questionRepo;
        private readonly ILogger<Question_APIController> _logger;
        public Question_APIController(ILogger<Question_APIController> logger)
        {
            _questionRepo = new QuestionRepository();
            _context = new STUDY4Context();
            _logger = logger;
        }

        [HttpPost("Get_AllQuestionOfLesson")]
        public async Task<IActionResult> Get_AllQuestionOfLesson(QuestionRequest _questionRequest)
        {
            if (_questionRequest.lessonId == null)
            {
                _logger.LogWarning("LessonId is null or empty in the request.");
                return BadRequest(new { status = 400, message = "LessonId is null or empty" });
            }

            try
            {
                var lessonTag = await _context.Lessons
                         .Where(l => l.LessonId == _questionRequest.lessonId)
                         .Select(l => l.Tag)
                         .FirstAsync();
                var lessonTagResponse = new
                {
                    lessonTag = lessonTag.TagId
                };
                var allQuestionOfLesson = await _questionRepo.GetAllQuestionsOfLesson(_questionRequest.lessonId);
                return Json(new { status = 200, message = "Get All Question Of Lesson Successful", allQuestionOfLesson, lessonTag = lessonTagResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _questionRequest.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }
        [HttpPost("Get_AllListenPicture")]
        public async Task<IActionResult> Get_AllListenPicture(OfLessonRequest _req)
        {
            if (_req.lessonId == null)
            {
                _logger.LogWarning("LessonId is null or empty in the request.");
                return BadRequest(new { status = 400, message = "LessonId is null or empty" });
            }

            try
            {
                var lessonTag = await _context.Lessons
                         .Where(l => l.LessonId == _req.lessonId)
                         .Select(l => l.Tag)
                         .FirstAsync();
                var lessonTagResponse = new
                {
                    lessonTag = lessonTag.TagId
                };
                var allQuestionOfLesson = await _questionRepo.GetAllQuestionsOfLesson(_req.lessonId);
                var listenPictureResponse = allQuestionOfLesson.Select(question => new ListenPictureResponse
                {
                    QuestionId = question.QuestionId,
                    QuestionAudio = question.QuestionAudio,
                    QuestionImage = question.QuestionImage,
                    QuestionParagraph = question.QuestionParagraph,
                    QuestionTranslate   = question.QuestionTranslate,
                    CorrectAnswer = question.CorrectAnswer,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                    OptionD = question.OptionD,
                });
                return Json(new { status = 200, message = "Get All Question Of Lesson Successful", data = listenPictureResponse, lessonTag = lessonTagResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _req.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }
        [HttpPost("Get_AllListen_Quest_Res")]
        public async Task<IActionResult> Get_AllListen_Quest_Res(OfLessonRequest _req)
        {
            if (_req.lessonId == null)
            {
                _logger.LogWarning("LessonId is null or empty in the request.");
                return BadRequest(new { status = 400, message = "LessonId is null or empty" });
            }

            try
            {
                var lessonTag = await _context.Lessons
                         .Where(l => l.LessonId == _req.lessonId)
                         .Select(l => l.Tag)
                         .FirstAsync();
                var lessonTagResponse = new
                {
                    lessonTag = lessonTag.TagId
                };
                var allQuestionOfLesson = await _questionRepo.GetAllQuestionsOfLesson(_req.lessonId);
                var listenPictureResponse = allQuestionOfLesson.Select(question => new ListenPictureResponse
                {
                    QuestionId = question.QuestionId,
                    QuestionAudio = question.QuestionAudio,
                    QuestionImage = question.QuestionImage,
                    QuestionParagraph = question.QuestionParagraph,
                    QuestionTranslate = question.QuestionTranslate,
                    CorrectAnswer = question.CorrectAnswer,
                    OptionA = question.OptionA,
                    OptionB = question.OptionB,
                    OptionC = question.OptionC,
                });
                return Json(new { status = 200, message = "Get All Question Of Lesson Successful", data = listenPictureResponse, lessonTag = lessonTagResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching vocab for lesson {LessonId}", _req.lessonId);
                return StatusCode(500, new { status = 500, message = "An error occurred while processing your request." });
            }
        }

    }
}
