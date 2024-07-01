using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Controllers.Admin;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Services.Request;
using study4_be.Services.Response;
using study4_be.Validation;

namespace study4_be.Controllers.API
{
    public class Exam_APIController : Controller
    {
        private STUDY4Context _context = new STUDY4Context();
        private readonly ILogger<CoursesController> _logger;
        private FireBaseServices _fireBaseServices;
        public Exam_APIController(ILogger<CoursesController> logger, FireBaseServices fireBaseServices)
        {
            _logger = logger;
            _fireBaseServices = fireBaseServices;
        }
        [HttpGet("Get_AllExams")]
        public async Task<ActionResult<IEnumerable<Course>>> Get_AllExams()
        {
            var exams = await _context.Courses.ToListAsync();
            return Json(new { status = 200, message = "Get Exams Successful", exams });
        }

        [HttpPost("Get_ExamDetailById")] // thieu user course 
        public async Task<ActionResult<IEnumerable<Course>>> Get_ExamDetailById(OfExamIdRequest _req)
        {
            var exams = await _context.Exams.Where(u => u.ExamId == _req.examId).FirstOrDefaultAsync();
                return Json(new { status = 200, message = "Get Exam Detail By Id ", exams });
        }
        [HttpGet("Get_ExamPart1")]
        public async Task<ActionResult> Get_ExamPart1(Part2Request _req)
        {
            try
            {
                var lessonTag = await _context.Lessons
                                       .Where(q => q.LessonId == _req.lessonId)
                                       .Select(t => t.TagId)
                                       .FirstOrDefaultAsync();
                if (lessonTag == _req.tagName)
                {
                    var questionPart2 = await _context.Questions
                                                      .Where(q => q.LessonId == _req.lessonId)
                                                      .ToListAsync();

                    var Part1Response = questionPart2.Select(p => new Part1Response
                    {
                        questionImage = p.QuestionImage,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                    });

                    return Json(new { status = 200, message = "Get_ExamPart1 successful", Part1Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = "Tag không khớp hoặc không tìm thấy." });
                }
            }catch(Exception ex) {
                return BadRequest(ex);
            }
         
        }
        [HttpGet("Get_ExamPart2")]
        public async Task<ActionResult> Get_ExamPart2(Part2Request _req)
        {
            try
            {
                var lessonTag = await _context.Lessons
                                       .Where(q => q.LessonId == _req.lessonId)
                                       .Select(t => t.TagId)
                                       .FirstOrDefaultAsync();
                if (lessonTag == _req.tagName)
                {
                    var questionPart2 = await _context.Questions
                                                      .Where(q => q.LessonId == _req.lessonId)
                                                      .ToListAsync();

                    var Part2Response = questionPart2.Select(p => new Part2Response
                    {
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                    });

                    return Json(new { status = 200, message = "Get_ExamPart2 successful", Part2Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = "Tag không khớp hoặc không tìm thấy." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpGet("Get_ExamPart3")]
        public async Task<ActionResult> Get_ExamPart3(Part2Request _req)
        {
            try
            {
                var lessonTag = await _context.Lessons
                                       .Where(q => q.LessonId == _req.lessonId)
                                       .Select(t => t.TagId)
                                       .FirstOrDefaultAsync();
                if (lessonTag == _req.tagName)
                {
                    var questionPart2 = await _context.Questions
                                                      .Where(q => q.LessonId == _req.lessonId)
                                                      .ToListAsync();

                    var Part3Response = questionPart2.Select(p => new Part3Response
                    {
                        questionText = p.QuestionText,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD =p.OptionD,
                    });

                    return Json(new { status = 200, message = "Get_ExamPart3 successful", Part3Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = "Tag không khớp hoặc không tìm thấy." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpGet("Get_ExamPart4")] // create question group 
        public async Task<ActionResult> Get_ExamPart4(Part2Request _req)
        {
            try
            {
                var lessonTag = await _context.Lessons
                                       .Where(q => q.LessonId == _req.lessonId)
                                       .Select(t => t.TagId)
                                       .FirstOrDefaultAsync();
                if (lessonTag == _req.tagName)
                {
                    var questionPart2 = await _context.Questions
                                                      .Where(q => q.LessonId == _req.lessonId)
                                                      .ToListAsync();

                    var Part3Response = questionPart2.Select(p => new Part3Response
                    {
                        questionText = p.QuestionText,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                    });

                    return Json(new { status = 200, message = "Get_ExamPart3 successful", Part3Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = "Tag không khớp hoặc không tìm thấy." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpGet("SubmitExam")] 
        public async Task<ActionResult> SubmitExam(SubmitExamRequest _req)
        {
            try
            {
                var existExam = _context.Exams.Where(e => e.ExamId == _req.examId).FirstOrDefaultAsync();
                var newUserExam = new UsersExam
                {
                    ExamId = _req.examId,
                    DateTime = DateTime.Now,
                    Process = 100,
                    Score = _req.score,
                    UserId = _req.userId,
                    // thieu user exan id
                };
                return Json(new { status = 200, message = "Submit Exam Successfull ", userExam = newUserExam });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
