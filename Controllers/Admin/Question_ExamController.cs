using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using study4_be.Models.ViewModel;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
namespace study4_be.Controllers.Admin
{
    public class Question_ExamController : Controller
    {
        private readonly ILogger<Question_ExamController> _logger;
        private FireBaseServices _firebaseServices;
        private GeneralAiAudioServices _generalAiAudioServices;
        public Question_ExamController(ILogger<Question_ExamController> logger, FireBaseServices firebaseServices)
        {
            _logger = logger;
            _firebaseServices = firebaseServices;
            _generalAiAudioServices = new GeneralAiAudioServices();
        }
        private readonly QuestionRepository _questionsRepository = new QuestionRepository();
        public Study4Context _context = new Study4Context();
        public async Task<IActionResult> Question_Exam_List()
        {
            try
            {
                var questions = await _context.Questions.Where(u => u.ExamId != null).ToListAsync();

                var questionViewModels = questions
                    .Select(ques => new QuestionExamListVIewModel
                    {
                        question = ques,
                    }).ToList();

                return View(questionViewModels);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error occurred while fetching vocabulary list.");

                // Handle the exception gracefully
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(new List<QuestionListViewModel>());
            }
        }  
        public async Task<IActionResult> Question_Exam_Create()
        {
            var exam = await _context.Exams.ToListAsync();

            var model = new QuestionExamCreateViewModel
            {
                question = new Question(),
                exam = exam.Select(c => new SelectListItem
                {
                    Value = c.ExamId.ToString(),
                    Text = $"{c.ExamName}"
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Question_Exam_Create(QuestionExamCreateViewModel questionViewModel, IFormFile QuestionImage, string selectedPart, string selectedCorrect)
        {
            try
            {
                var firebaseBucketName = _firebaseServices.GetFirebaseBucketName();
                var uniqueId = Guid.NewGuid().ToString();
                string firebaseUrl = null;
                if (QuestionImage != null)
                {
                    var uniqueIdForQuestionImage = Guid.NewGuid().ToString();
                    var imgFilePath = ($"IMG{uniqueIdForQuestionImage}.jpg");
                    firebaseUrl = await _firebaseServices.UploadFileToFirebaseStorageAsync(QuestionImage, imgFilePath, firebaseBucketName);
                }
                if (selectedPart == null)
                {
                    selectedPart = "Part 1";
                }
                if (selectedCorrect == null)
                {
                    selectedCorrect = "A";
                }
                var question = new Question
                {
                    QuestionId = questionViewModel.question.QuestionId,
                    QuestionText = questionViewModel.question.QuestionText,
                    QuestionParagraph = questionViewModel.question.QuestionParagraph,
                    QuestionImage = !string.IsNullOrEmpty(firebaseUrl) ? firebaseUrl : (string)null,
                    CorrectAnswer = selectedCorrect,
                    OptionA = questionViewModel.question.OptionA,
                    OptionB = questionViewModel.question.OptionB,
                    OptionC = questionViewModel.question.OptionC,
                    OptionD = questionViewModel.question.OptionD,
                    ExamId = questionViewModel.question.ExamId,
                    QuestionTag = selectedPart,
                };

                await _context.AddAsync(question);
                await _context.SaveChangesAsync();

                return RedirectToAction("Question_Exam_List", "Question_Exam");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                questionViewModel.exam = _context.Exams.Select(c => new SelectListItem
                {
                    Value = c.ExamId.ToString(),
                    Text = c.ExamName.ToString()
                }).ToList();

                return View(questionViewModel);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionExamById(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            return Ok(question);
        }
        [HttpGet]
        public IActionResult Question_Exam_Delete(int id)
        {
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Exam with ID {id} not found for delete.");
                return NotFound($"Exam with ID {id} not found.");
            }
            return View(question);
        }

        [HttpPost, ActionName("Question_Exam_Delete")]
        public IActionResult Question_Exam_DeleteConfirmed(int id)
        {
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Exam with ID {id} not found for deletion.");
                return NotFound($"Exam with ID {id} not found.");
            }

            try
            {
                _context.Questions.Remove(question);
                _context.SaveChanges();
                return RedirectToAction("Question_Exam_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting exam with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(question);
            }
        }
        [HttpGet]
        public IActionResult Question_Exam_Edit(int id)
        {
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }
            var exams = _context.Exams.ToList();
            var selectListTags = exams.Select(exams => new SelectListItem
            {
                Value = exams.ExamId.ToString(),
                Text = exams.ExamId
            }).ToList();

            var viewModel = new QuestionExamEditViewModel
            {
                question = question,
                exam = selectListTags
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Question_Exam_Edit(QuestionExamEditViewModel questionViewModel, IFormFile QuestionImage)
        {

            try
            {
                var courseToUpdate = _context.Questions.FirstOrDefault(c => c.QuestionId == questionViewModel.question.QuestionId);
                if (QuestionImage != null && QuestionImage.Length > 0)
                {
                    var firebaseBucketName = _firebaseServices.GetFirebaseBucketName();
                    // Delete the old image from Firebase Storage
                    if (!string.IsNullOrEmpty(courseToUpdate.QuestionImage))
                    {
                        // Extract the file name from the URL
                        var oldFileName = Path.GetFileName(new Uri(courseToUpdate.QuestionImage).LocalPath);
                        await _firebaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName);
                    }
                    var uniqueId = Guid.NewGuid().ToString();
                    var imgFilePath = ($"IMG{uniqueId}.jpg");
                    string firebaseUrl = await _firebaseServices.UploadFileToFirebaseStorageAsync(QuestionImage, imgFilePath, firebaseBucketName);
                    courseToUpdate.QuestionImage = firebaseUrl;
                    courseToUpdate.QuestionText = questionViewModel.question.QuestionText;
                    courseToUpdate.QuestionTextMean = questionViewModel.question.QuestionText;
                    courseToUpdate.QuestionParagraph = questionViewModel.question.QuestionParagraph;
                    courseToUpdate.OptionA = questionViewModel.question.OptionA;
                    courseToUpdate.OptionB = questionViewModel.question.OptionB;
                    courseToUpdate.OptionC = questionViewModel.question.OptionC;
                    courseToUpdate.OptionD = questionViewModel.question.OptionD;
                    courseToUpdate.QuestionTag = questionViewModel.question.QuestionTag;
                    courseToUpdate.CorrectAnswer = questionViewModel.question.CorrectAnswer;
                    courseToUpdate.LessonId = questionViewModel.question.LessonId;
                    _context.SaveChanges();
                    return RedirectToAction("Question_Exam_List");
                }
                else
                {
                    courseToUpdate.QuestionImage = questionViewModel.question.QuestionImage;
                    courseToUpdate.QuestionText = questionViewModel.question.QuestionText;
                    courseToUpdate.QuestionTextMean = questionViewModel.question.QuestionText;
                    courseToUpdate.QuestionParagraph = questionViewModel.question.QuestionParagraph;
                    courseToUpdate.OptionA = questionViewModel.question.OptionA;
                    courseToUpdate.OptionB = questionViewModel.question.OptionB;
                    courseToUpdate.OptionC = questionViewModel.question.OptionC;
                    courseToUpdate.OptionD = questionViewModel.question.OptionD;
                    courseToUpdate.QuestionTag = questionViewModel.question.QuestionTag;
                    courseToUpdate.CorrectAnswer = questionViewModel.question.CorrectAnswer;
                    courseToUpdate.LessonId = questionViewModel.question.LessonId;
                    _context.SaveChanges();
                    return RedirectToAction("Question_Exam_List");
                }
            }

            catch (Exception ex)
            {
                _logger.LogError($"Error updating exam with ID {questionViewModel.question.QuestionId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the exam.");
            }
            return View(questionViewModel);
        }
        public IActionResult Question_Exam_Details(int id)
        {
            return View(_context.Questions.FirstOrDefault(c => c.QuestionId == id));
        }
    }
}
