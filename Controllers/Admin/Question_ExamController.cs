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
        public Question_ExamController(ILogger<Question_ExamController> logger, FireBaseServices firebaseServices)
        {
            _logger = logger;
            _firebaseServices = firebaseServices;
        }
        private readonly QuestionRepository _questionsRepository = new QuestionRepository();
        private Study4Context _context = new Study4Context();
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
        public async Task<IActionResult> Question_Exam_Create(QuestionExamCreateViewModel questionViewModel, IFormFile? QuestionImage, string selectedPart, string selectedCorrect)
        {
            // Custom validation: Check if all question fields are null or empty
            if (string.IsNullOrEmpty(questionViewModel.question.QuestionText) &&
                string.IsNullOrEmpty(questionViewModel.question.QuestionParagraph) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionA) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionB) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionC) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionD))
            {
                ModelState.AddModelError("", "Please fill in at least one field before submitting.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new unit.");

                questionViewModel.exam = _context.Exams.Select(c => new SelectListItem
                {
                    Value = c.ExamId.ToString(),
                    Text = c.ExamName.ToString()
                }).ToList();

                return View(questionViewModel);
            }
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

        public async Task<IActionResult> GetQuestionExamById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
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
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Question not found for deletion.");
                return NotFound($"Question not found.");
            }
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Question not found for delete.");
                return NotFound($"Question not found.");
            }
            return View(question);
        }

        [HttpPost, ActionName("Question_Exam_Delete")]
        public IActionResult Question_Exam_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Question not found for deletion.");
                return NotFound($"Question not found.");
            }
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Question not found for deletion.");
                return NotFound($"Question not found.");
            }

            try
            {
                _context.Questions.Remove(question);
                _context.SaveChanges();
                return RedirectToAction("Question_Exam_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting question: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the question.");
                return View(question);
            }
        }
        [HttpGet]
        public IActionResult Question_Exam_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "questionId is invalid" });
            }
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
        public async Task<IActionResult> Question_Exam_Edit(QuestionExamEditViewModel questionViewModel, IFormFile? QuestionImage)
        {
            if(ModelState.IsValid) 
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
                    _logger.LogError($"Error updating question: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the question.");
                }
            }
            return View(questionViewModel);
        }
        public IActionResult Question_Exam_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Question ID.");
                TempData["ErrorMessage"] = "The specified Question was not found.";
                return RedirectToAction("Question_Exam_List", "Question_Exam");
            }

            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);

            // If no container is found, return to the list with an error
            if (question == null)
            {
                TempData["ErrorMessage"] = "The specified Question was not found.";
                return RedirectToAction("Question_Exam_List", "Question_Exam");
            }
            return View(question);
        }
    }
}
