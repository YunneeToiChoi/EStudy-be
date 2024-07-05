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
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<ActionResult<IEnumerable<Exam>>> Get_AllExams()
        {
            try
            {
                var exams = await _context.Exams.ToListAsync();
                return Json(new { status = 200, message = "Get Exams Successful", exams });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        
        }

        [HttpPost("Get_ExamDetailById")]
        public async Task<ActionResult<IEnumerable<Exam>>> Get_ExamDetailById(OfExamUserIdRequest _req)
        {
            try
            {
                var exams = await _context.Exams.Where(u => u.ExamId == _req.examId).FirstOrDefaultAsync();
                var userExam = await _context.UsersExams.Where(u => u.UserId == _req.userId && u.ExamId == _req.examId).ToListAsync();
                if (userExam != null)
                {
                    return Json(new { status = 200, message = "Get Exam Detail By Id ", exams, userExam });
                }
                else
                {
                    return Json(new { status = 200, message = "Get Exam Detail By Id ", exams, userExam = "User hadn't learn this exam before " });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }
        [HttpPost("Get_AudioExam")]
        public async Task<ActionResult<IEnumerable<Exam>>> Get_AudioExam(OfExamIdRequest _req)
        {
            try
            {
                var examAudio = await _context.Exams.Where(u => u.ExamId == _req.examId).Select(a=>a.ExamAudio).FirstOrDefaultAsync();
                return Json(new { status = 200, message = "Get Exam Detail By Id ", examAudio });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("Get_ExamPart1")]
        public async Task<ActionResult> Get_ExamPart1(Part2Request _req)
        {
            try
            {
                var questionPart= await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                    int number = 0;
                    var part1Response = questionPart.Select(p => new Part1Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionImage = p.QuestionImage,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart1 successful", part1Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }catch(Exception ex) {
                return BadRequest(ex);
            }
         
        }
        [HttpPost("Get_ExamPart2")]
        public async Task<ActionResult> Get_ExamPart2(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                    int number = 7;
                    var part2Response = questionPart.Select(p => new Part2Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,

                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart2 successful", part2Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpPost("Get_ExamPart3")]
        public async Task<ActionResult> Get_ExamPart3(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                     int number = 32;
                    var part3Response = questionPart.Select(p => new Part3Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionText = p.QuestionText,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                        
                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart3 successful", part3Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpPost("Get_ExamPart4")] // create question group 
        public async Task<ActionResult> Get_ExamPart4(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                    int number = 71;
                    var part4Response = questionPart.Select(p => new Part4Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionText = p.QuestionText,
                        questionImage = p.QuestionImage,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,

                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart4 successful", part4Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }   
        [HttpPost("Get_ExamPart5")]
        public async Task<ActionResult> Get_ExamPart5(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                    int number = 101;
                    var part5Response = questionPart.Select(p => new Part5Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionText = p.QuestionText,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart5 successful", part5Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpPost("Get_ExamPart6")]
        public async Task<ActionResult> Get_ExamPart6(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null && questionPart.Any())
                {
                    int number = 131;
                    var part6Responses = questionPart.Select(p => new Part6Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionImage = p.QuestionImage,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                    }).ToList();

                    return Json(new { status = 200, message = "Get_ExamPart6 successful", part6Responses });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }
        [HttpPost("Get_ExamPart7")]
        public async Task<ActionResult> Get_ExamPart7(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                    int number = 147;
                    var part7Response = questionPart.Select(p => new Part7Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionText = p.QuestionText,
                        questionImage = p.QuestionImage,
                        correctAnswear = p.CorrectAnswer,
                        optionA = p.OptionA,
                        optionB = p.OptionB,
                        optionC = p.OptionC,
                        optionD = p.OptionD,
                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart7 successful", part7Response });
                }
                else
                {
                    return BadRequest(new { status = 404, message = $"Tag không khớp hoặc Không có Exam không có {_req.tagName}." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("SubmitExam")] 
        public async Task<ActionResult> SubmitExam(SubmitExamRequest _req)
        {
            try
            {
                var existExam = _context.Exams.Where(e => e.ExamId == _req.examId).FirstOrDefaultAsync();
                var newUserExam = new UsersExam
                {
                    ExamId = _req.examId,
                    DateTime = DateTime.Now,
                    State = true,
                    Score = _req.score,
                    UserId = _req.userId,
                    UserExamId = Guid.NewGuid().ToString(),
            };
                return Json(new { status = 200, message = "Submit Exam Successfull ", userExam = newUserExam });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        } 
        [HttpPost("StopExam")] 
        public async Task<ActionResult> StopExam(SubmitExamRequest _req)
        {
            try
            {
                var existExam = _context.Exams.Where(e => e.ExamId == _req.examId).FirstOrDefaultAsync();
                var newUserExam = new UsersExam
                {
                    ExamId = _req.examId,
                    DateTime = DateTime.Now,
                    State = false,
                    Score = _req.score,
                    UserId = _req.userId,
                    UserExamId = Guid.NewGuid().ToString(),
            };
                return Json(new { status = 200, message = "Submit Exam Successfull ", userExam = newUserExam });
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        //[HttpPost("LearnAgain")]
        //public async Task<ActionResult> LearnAgain(LearnAgainRequest _req)
        //{
        //    try
        //    {
        //        var userExam = _context.UsersExams.Where(e => e.UserExamId == _req.userExamId).FirstOrDefaultAsync();
        //        var newUserExam = new UsersExam
        //        {
        //            ExamId = _req.examId,
        //            DateTime = DateTime.Now,
        //            State = false,
        //            Score = _req.score,
        //            UserId = _req.userId,
        //            UserExamId = Guid.NewGuid().ToString(),
        //        };
        //        return Json(new { status = 200, message = "Submit Exam Successfull ", userExam = newUserExam });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
        //}
        public IActionResult Index()
        {
            return View();
        }
    }
}
