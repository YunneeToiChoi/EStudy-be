﻿using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaCl;
using study4_be.Controllers.Admin;
using study4_be.Interface;
using study4_be.Interface.Exam;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Services.Exam;
using study4_be.Services.User;
using study4_be.Validation;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Exam_APIController : Controller
    {
        private readonly Study4Context _context;
        private readonly IExamService _examService;
        private readonly IWritingService _writingService;

        public Exam_APIController(Study4Context context, IWritingService writingService, IExamService examService)
        {
            _context = context;
            _writingService = writingService;
            _examService = examService; 
        }
        [HttpGet("Get_AllExams")]
        public async Task<ActionResult<IEnumerable<Exam>>> Get_AllExams()
        {
            try
            {
                var exams = await _examService.GetAllExamsAsync();
                return Json(new { status = 200, message = "Get Exams Successful", exams });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }  
        [HttpPost("Get_UserExams")]
        public async Task<ActionResult<IEnumerable<Exam>>> Get_UserExams(OfUserIdRequest _req)
        {
            try
            {
                var exams = await _examService.GetUserExamsAsync(_req.userId);
                return Json(new { status = 200, message = "Get User Exams Successful", exams });
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
                var distinctUserCount = await _context.UsersExams
                       .Where(e => e.ExamId == _req.examId)
                       .Select(ue => ue.UserId).Distinct().CountAsync();

                var amountTest = await _context.UsersExams.Where(e => e.ExamId == _req.examId).CountAsync();
                if (userExam != null && userExam.Count > 0)
                {

                    var userExamResponse = userExam.Select(ue => new UserExamResponse
                    {
                        userId = ue.UserId,
                        userTime = ue.UserTime.HasValue ? ConvertSecondsToHMS(ue.UserTime.Value) : "N/A",
                        userExamId = ue.UserExamId,
                        examId = ue.ExamId,
                        dateTime = ue.DateTime.HasValue ? ue.DateTime.Value.ToString("yyyy-MM-dd") : "N/A",
                        state = ue.State,
                        score = ue.Score
                    });
                    return Json(new
                    {
                        status = 200,
                        message = "Get Exam Detail By Id",
                        exams,
                        userExamResponse,
                        totalUsers = distinctUserCount,
                        totalAmountTest = amountTest
                    });
                }
                else
                {
                    return Json(new
                    {
                        status = 200,
                        message = "Get Exam Detail By Id",
                        exams,
                        userExamResponse = "",
                        totalUsers = distinctUserCount,
                        totalAmountTest = amountTest
                    });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("OutstandingExamsUserNotTest")]
        public async Task<ActionResult<IEnumerable<Exam>>> OutstandingExamsUserNotTest(OfUserIdNotRequiredRequest _req)
        {
            try
            {
                // List to hold exams
                IEnumerable<Exam> outstandingExams;

                if (!string.IsNullOrEmpty(_req.userId))
                {
                    // Get exams the user has not tested
                    outstandingExams = await GetOutstandingExamsUserNotTestAsync(_req.userId);
                }
                else
                {
                    // If user ID is null, return outstanding exams for guests
                    outstandingExams = await GetOutstandingExamsForGuestAsync(4);
                }
                
                // Create a list to hold the exam details
                var examDetailsList = new List<ExamPreviewDetails>();
                
                // Now process each exam
                foreach (var e in outstandingExams)
                {
                    int examCommentsCounts = 100; // default because ratings model hasn't relationship with exam model
                    
                    // get amount user test this exam
                    int usersExamsCounts = await _context.UsersExams
                        .Where(ue=>ue.ExamId == e.ExamId)
                        .CountAsync();
                    
                    // return exam details data
                    var examDetails = new ExamPreviewDetails
                    {
                        ExamId = e.ExamId,
                        ExamName = e.ExamName,
                        ExamImage = e.ExamImage,
                        TotalComments = 100,
                        TotalMinutes = 120, // default because total minutes not exist in models
                        TotalUsersTest = usersExamsCounts
                    };

                    examDetailsList.Add(examDetails);
                }

                // Return the formatted response
                return Json(new
                {
                    outstandingExams = examDetailsList,
                    message = "Success",
                    error = (string)null,
                    statusCode = 200
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { statusCode = 500, message = "Error", error = ex.Message });
            }
        }
        // Private method to get outstanding exams for guests
        private async Task<IEnumerable<Exam>> GetOutstandingExamsForGuestAsync(int amountOutstanding)
        {
            return await _context.Exams
                .GroupBy(e => e)
                .Select(g => new { Exam = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(amountOutstanding)
                .Select(g => g.Exam)
                .ToListAsync();
        }

        // Private method to get outstanding exams the user hasn't tested
        private async Task<IEnumerable<Exam>> GetOutstandingExamsUserNotTestAsync(string userId)
        {
            return await _context.Exams
                .Where(e => !_context.UsersExams.Any(ue => ue.UserId == userId && ue.ExamId == e.ExamId))
                .GroupBy(e => e)
                .Select(g => new { Exam = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(4)
                .Select(g => g.Exam)
                .ToListAsync();
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
                    int number = 1;
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
        [HttpPost("Get_ExamPart8")]
        public async Task<ActionResult> Get_ExamPart8(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                                       .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                                       .ToListAsync();
                if (questionPart != null)
                {
                    int number = 201;
                    var part8Response = questionPart.Select(p => new Part8Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionText = p.QuestionParagraph,
                        questionImage = p.QuestionImage,
                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart8 successful", part8Response });
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

        [HttpPost("Get_ExamPart9")]
        public async Task<ActionResult> Get_ExamPart9(Part2Request _req)
        {
            try
            {
                var questionPart = await _context.Questions
                    .Where(q => q.ExamId == _req.examId && q.QuestionTag == _req.tagName)
                    .ToListAsync();
                if (questionPart != null)
                {
                    int number = 209;
                    var part9Response = questionPart.Select(p => new Part9Response
                    {
                        number = number++,
                        questionId = p.QuestionId,
                        questionText = p.QuestionText + ": " +p.QuestionParagraph,
                        questionImage = p.QuestionImage,
                    }).ToList();
                    return Json(new { status = 200, message = "Get_ExamPart9 successful", part9Response });
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
                if (string.IsNullOrEmpty(_req.examId) || string.IsNullOrEmpty(_req.userId) || _req.answer == null || !_req.answer.Any())
                {
                    return BadRequest("Invalid input data");
                }

                var existExam = await _context.Exams.FirstOrDefaultAsync(e => e.ExamId == _req.examId);
                if (existExam == null)
                {
                    return BadRequest("Exam not found");
                }
                var userExists = await _context.Users.AnyAsync(u => u.UserId == _req.userId);
                if (!userExists)
                {
                    // Xử lý trường hợp người dùng không tồn tại
                    throw new Exception("User not found.");
                }

                var userExamId = Guid.NewGuid().ToString();
                var newUserExam = new UsersExam
                {
                    ExamId = _req.examId,
                    DateTime = DateTime.Now,
                    State = true,
                    Score = _req.score,
                    UserId = _req.userId,
                    UserExamId = userExamId,
                    UserTime = 7200 - _req.userTime
                    
                };
                await _context.UsersExams.AddAsync(newUserExam);
                await _context.SaveChangesAsync();
                var userAnswers = _req.answer.Select(answer => new UserAnswer
                {
                    UserExamId = userExamId,
                    QuestionId = answer.QuestionId,
                    Answer = answer.Answer
                }).ToList();

                await _context.UserAnswers.AddRangeAsync(userAnswers);
                await _context.SaveChangesAsync();
                // Step 1: Retrieve relevant QuestionIds for Part 8 and the specified examId
                var questionIds = await _context.Questions
                    .Where(q => q.ExamId == existExam.ExamId && q.QuestionTag == "Part 8")
                    .Select(q => q.QuestionId)
                    .ToListAsync();

                // Step 2: Retrieve UserAnswers based on UserExamId and the filtered QuestionIds
                var writingQuestions = await _context.UserAnswers
                    .Where(ua => ua.UserExamId == newUserExam.UserExamId && questionIds.Contains(ua.QuestionId))
                    .ToListAsync();
                 
                int totalQuestions = 200;
                int answeredQuestions = _req.answer.Count - writingQuestions.Count;
                int correctAnswers = _req.answer.Count(a => a.State);
                int incorrectAnswers = totalQuestions - answeredQuestions + _req.answer.Count(a => !a.State);

                int score = (int)((double)correctAnswers / totalQuestions * 990);
                
                var writingScore = await CalculateWritingScore(writingQuestions);
                
                var finalWritingScore = writingScore.Sum(respone => respone.score);
                
                newUserExam.Score = score;
                newUserExam.WritingScore = finalWritingScore;
                await _context.SaveChangesAsync();

                var responseUserExam = new
                {
                    examId = newUserExam.ExamId,
                    dateTime = newUserExam.DateTime,
                    state = newUserExam.State,
                    score = newUserExam.Score,
                    userId = newUserExam.UserId,
                    userExamId = newUserExam.UserExamId, 
                    userTime = newUserExam.UserTime,
                    listenAndReadingScore = score,
                    writingScore = newUserExam.WritingScore,
                    incorrectAnswers
                };
                return Json(new { status = 200, message = "Submit Exam Successful", responseUserExam });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        [HttpGet("ReviewQuestions/userExamId={userExamId}")]
        public async Task<ActionResult> ReviewQuestions(string userExamId)
        {
            try
            {
                // Lấy thông tin của UserExam và câu trả lời của người dùng
                var userExam = await _context.UsersExams
                    .Include(ue => ue.Exam) // dư
                    .Include(ue => ue.User) // dư
                    .FirstOrDefaultAsync(ue => ue.UserExamId == userExamId);

                if (userExam == null)
                {
                    return NotFound("Không tìm thấy thông tin bài thi của người dùng.");
                }

                // Lấy danh sách câu hỏi và câu trả lời của người dùng
                var userAnswers = await _context.UserAnswers
                    .Where(ua => ua.UserExamId == userExamId)
                    .ToListAsync();

                // Lấy danh sách các câu hỏi từ bài thi
                var questionIds = userAnswers.Select(ua => ua.QuestionId).ToList();
                var questions = await _context.Questions
                    .Where(q => questionIds.Contains(q.QuestionId))
                    .ToListAsync();

                // Chuẩn bị dữ liệu để trả về
                var reviewData = new
                {
                    UserExam = new
                    {
                        userExam.UserExamId,
                        userExam.UserId,
                        userExam.ExamId,
                        userExam.DateTime,
                        userExam.Score
                    },
                    ExamInfo = new
                    {
                        userExam.Exam.ExamId,
                        userExam.Exam.ExamName,
                        userExam.Exam.ExamAudio,
                        userExam.Exam.ExamImage
                    },      
                    Questions = questions.Select(q => new
                    {
                       questionId =  q.QuestionId,
                       questionText =  q.QuestionText,
                       questionParagraph =  q.QuestionParagraph,
                       questionImage =  q.QuestionImage,
                       optionA =  q.OptionA,
                       optionB =  q.OptionB,
                       optionC =  q.OptionC,
                       optionD =  q.OptionD,
                       correctAnswer = q.CorrectAnswer,
                       userAnswer = userAnswers.Find(ua => ua.QuestionId == q.QuestionId)?.Answer,
                       comment = userAnswers.Find(ua => ua.QuestionId == q.QuestionId)?.Comment,
                       explain = userAnswers.Find(ua => ua.QuestionId == q.QuestionId)?.Explain,
                       state = userAnswers.Find(ua => ua.QuestionId == q.QuestionId)?.Answer == q.CorrectAnswer
                    }).ToList()
                };
                return Ok(reviewData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        private static string ConvertSecondsToHMS(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int seconds = totalSeconds % 60;

            return $"{hours} giờ {minutes} phút {seconds} giây";
        }
        private async Task<List<WritingScore>> CalculateWritingScore(List<UserAnswer> writingAnswers)
        {
            List<WritingScore> writingScoreResponses = new List<WritingScore>();

            for (int i = 0; i < writingAnswers.Count; i++)
            {
                WritingScore writingScoreRespone = new WritingScore();

                if (!string.IsNullOrEmpty(writingAnswers[i].Answer))
                {
                    var question = await _context.Questions.FindAsync(writingAnswers[i].QuestionId);
                    string content = $"Question : {question.QuestionParagraph} \n Answer : {writingAnswers[i].Answer}";

                    int difficulty = i < 5 ? 10 : (i < 7 ? 25 : 100);
                    
                    int modelIndex = i % 4 + 1;

                    // Gọi hàm ScoringWritingAsync với model được chọn
                    writingScoreRespone = await _writingService.ScoringWritingAsync(difficulty, content, modelIndex);

                    writingAnswers[i].Comment = writingScoreRespone.comment;
                    writingAnswers[i].Explain = writingScoreRespone.explain;
                }
                else
                {
                    // Trường hợp không có câu trả lời
                    writingScoreRespone.score = 0; // Hoặc giá trị mặc định tùy theo yêu cầu
                }

                writingScoreResponses.Add(writingScoreRespone); // Lưu WritingScoreRespone vào danh sách
            }

            return writingScoreResponses; // Trả về danh sách WritingScoreRespone
        }
        
        [HttpDelete("Delete_AllUsersExam")]
        public async Task<IActionResult> Delete_AllUsersExam()
        {
            try
            {
                var users = await _context.UsersExams.ToListAsync();

                if (users.Any())
                {
                    _context.UsersExams.RemoveRange(users);
                    await _context.SaveChangesAsync(); // Ensures changes are saved to the database
                }

                return Json(new { status = 200, message = "Delete Users Exam Successful" });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, message = e.Message }); // Structured error response
            }

        }  
        [HttpDelete("Delete_AllUserAnswer")]
        public async Task<IActionResult> Delete_AllUserAnswer()
        {
            try
            {
                var users = await _context.UserAnswers.ToListAsync();

                if (users.Any())
                {
                    _context.UserAnswers.RemoveRange(users);
                    await _context.SaveChangesAsync(); // Ensures changes are saved to the database
                }

                return Json(new { status = 200, message = "Delete Users Exam Successful" });
            }
            catch (Exception e)
            {
                return BadRequest(new { status = 400, message = e.Message }); // Structured error response
            }

        }
    }
}
