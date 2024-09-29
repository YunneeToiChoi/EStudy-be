using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    public class QuestionController : Controller
    {
        private readonly ILogger<QuestionController> _logger;
        private FireBaseServices _firebaseServices;
        private GeneralAiAudioServices _generalAiAudioServices;
        public QuestionController(ILogger<QuestionController> logger, FireBaseServices firebaseServices)
        {
            _logger = logger;
            _firebaseServices = firebaseServices;
            _generalAiAudioServices = new GeneralAiAudioServices();
        }
        private readonly QuestionRepository _questionsRepository = new QuestionRepository();
        private readonly Study4Context _context = new Study4Context();
        public async Task<IActionResult> DeleteAllQuestions()
        {
            await _questionsRepository.DeleteAllQuestionsAsync();
            return Json(new { status = 200, message = "Delete Questions Successful" });
        }
        public async Task<IActionResult> Question_List()
        {
            try
            {
                var questions = await _context.Questions
                        .Where(q => q.ExamId == null)
                    .Include(v => v.Lesson)
                    .ThenInclude(l => l.Container)
                        .ThenInclude(c => c.Unit)
                            .ThenInclude(u => u.Course)
                    .ToListAsync();

                var questionViewModels = questions
                    .Select(ques => new QuestionListViewModel
                    {
                        question = ques,
                        courseName = ques.Lesson?.Container?.Unit?.Course?.CourseName ?? "N/A",
                        unitTittle = ques.Lesson?.Container?.Unit?.UnitTittle ?? "N/A",
                        containerTittle = ques.Lesson?.Container?.ContainerTitle ?? "N/A",
                        lessonTittle = ques.Lesson?.LessonTitle ?? "N/A",
                        tag = ques.Lesson?.TagId ?? "N/A",
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
        public async Task<IActionResult> Question_Create()
        {
            var lessons = await _context.Lessons
                           .Include(l => l.Container)
                               .ThenInclude(c => c.Unit)
                                   .ThenInclude(u => u.Course)
                           .ToListAsync();

            var model = new QuestionCreateViewModel
            {
                question = new Question(),
                lesson = lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = $"{c.LessonTitle} - Container: {(c.Container?.ContainerTitle ?? "N/A")} - Unit: {(c.Container?.Unit?.UnitTittle ?? "N/A")} - Course: {(c.Container?.Unit?.Course?.CourseName ?? "N/A")} - TAG: {(c.TagId ?? "N/A")}"
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Question_Create(QuestionCreateViewModel questionViewModel, IFormFile? QuestionImage,string selectedCorrect)
        {
            // Custom validation: Check if all question fields are null or empty
            if (string.IsNullOrEmpty(questionViewModel.question.QuestionText) &&
                string.IsNullOrEmpty(questionViewModel.question.QuestionTextMean) &&
                string.IsNullOrEmpty(questionViewModel.question.QuestionParagraph) &&
                string.IsNullOrEmpty(questionViewModel.question.QuestionParagraphMean) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionA) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionB) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionC) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionD) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionMeanA) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionMeanB) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionMeanC) &&
                string.IsNullOrEmpty(questionViewModel.question.OptionMeanD))
            {
                ModelState.AddModelError("", "Please fill in at least one field before submitting.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new question.");

                questionViewModel.lesson = await _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToListAsync();

                return View(questionViewModel);
            }
            try
            {
                var firebaseBucketName = _firebaseServices.GetFirebaseBucketName();
                var uniqueId = Guid.NewGuid().ToString(); 

                var audioFilePath = Path.Combine(Path.GetTempPath(), $"QUESTION({uniqueId}).wav");

                if (questionViewModel.question.QuestionParagraph == null)
                {
                    _logger.LogError("Cannot generate AI question because there is no Paragraph.");
                    ModelState.AddModelError("", "Cannot generate AI question because there is no Paragraph.");
                }
                _generalAiAudioServices.GenerateAudio(questionViewModel.question.QuestionParagraph, audioFilePath);
                var audioBytes = System.IO.File.ReadAllBytes(audioFilePath);
                var audioUrl = await _generalAiAudioServices.UploadFileToFirebaseStorageAsync(audioBytes, $"QUESTION({uniqueId}).wav", firebaseBucketName);
                string firebaseUrl = null;
                if(QuestionImage!=null)
                {
                    var uniqueIdForQuestionImage = Guid.NewGuid().ToString();
                    var imgFilePath = ($"IMG{uniqueIdForQuestionImage}.jpg");
                    firebaseUrl = await _firebaseServices.UploadFileToFirebaseStorageAsync(QuestionImage, imgFilePath, firebaseBucketName);
                }

                // Delete the temporary file after uploading
                System.IO.File.Delete(audioFilePath);
                if(selectedCorrect == null)
                {
                    selectedCorrect = "A";
                }
                var question = new Question
                {
                    QuestionId = questionViewModel.question.QuestionId,
                    QuestionText = questionViewModel.question.QuestionText,
                    QuestionTextMean = questionViewModel.question.QuestionTextMean,
                    QuestionParagraph = questionViewModel.question.QuestionParagraph,
                    QuestionParagraphMean = questionViewModel.question.QuestionParagraphMean,
                    QuestionAudio = audioUrl,
                    QuestionTranslate = questionViewModel.question.QuestionTranslate,
                    QuestionImage = !string.IsNullOrEmpty(firebaseUrl) ? firebaseUrl : (string)null,
                    CorrectAnswer = selectedCorrect,
                    OptionA = questionViewModel.question.OptionA,
                    OptionB = questionViewModel.question.OptionB,
                    OptionC = questionViewModel.question.OptionC,
                    OptionD = questionViewModel.question.OptionD,
                    OptionMeanA = questionViewModel.question.OptionMeanA,
                    OptionMeanB = questionViewModel.question.OptionMeanB,
                    OptionMeanC = questionViewModel.question.OptionMeanC,
                    OptionMeanD = questionViewModel.question.OptionMeanD,
                    LessonId = questionViewModel.question.LessonId,
                };

                await _context.AddAsync(question);
                await _context.SaveChangesAsync();

                return RedirectToAction("Question_List", "Question");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new question.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                questionViewModel.lesson = await _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToListAsync();

                return View(questionViewModel);
            }
        }

        public async Task<IActionResult> GetQuestionById(int id)
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
        public async Task<IActionResult> Question_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Question not found for deletion.");
                return NotFound($"Question not found.");
            }
            var question = await _context.Questions.FirstOrDefaultAsync(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Question not found for delete.");
                return NotFound($"Question not found.");
            }
            return View(question);
        }

        [HttpPost, ActionName("Question_Delete")]
        public async Task<IActionResult> Question_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Question not found for deletion.");
                return NotFound($"Question not found.");
            }
            var question = await _context.Questions.FirstOrDefaultAsync(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Question not found for deletion.");
                return NotFound($"Question not found.");
            }

            try
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                return RedirectToAction("Question_Exam_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting Question: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the Question.");
                return View(question);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Question_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "questionId is invalid" });
            }
            var question = await _context.Questions.FirstOrDefaultAsync(c => c.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> Question_Edit(Question question, IFormFile? QuestionImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var courseToUpdate = await _context.Questions.FirstOrDefaultAsync(c => c.QuestionId == question.QuestionId);
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
                        courseToUpdate.QuestionText = question.QuestionText;
                        courseToUpdate.QuestionTextMean = question.QuestionTextMean;
                        courseToUpdate.QuestionParagraph = question.QuestionParagraph;
                        courseToUpdate.QuestionParagraphMean = question.QuestionParagraphMean;
                        courseToUpdate.QuestionTranslate = question.QuestionTranslate;
                        courseToUpdate.OptionA = question.OptionA;
                        courseToUpdate.OptionMeanA = question.OptionMeanA;
                        courseToUpdate.OptionB = question.OptionB;
                        courseToUpdate.OptionMeanB = question.OptionMeanB;
                        courseToUpdate.OptionC = question.OptionC;
                        courseToUpdate.OptionMeanC = question.OptionMeanC;
                        courseToUpdate.OptionD = question.OptionD;
                        courseToUpdate.OptionMeanD = question.OptionMeanD;
                        courseToUpdate.CorrectAnswer = question.CorrectAnswer;
                        _context.SaveChanges();
                        return RedirectToAction("Question_List");
                    }
                    else
                    {
                        courseToUpdate.QuestionImage = courseToUpdate.QuestionImage;
                        courseToUpdate.QuestionText = question.QuestionText;
                        courseToUpdate.QuestionTextMean = question.QuestionTextMean;
                        courseToUpdate.QuestionParagraph = question.QuestionParagraph;
                        courseToUpdate.QuestionParagraphMean = question.QuestionParagraphMean;
                        courseToUpdate.QuestionTranslate = question.QuestionTranslate;
                        courseToUpdate.OptionA = question.OptionA;
                        courseToUpdate.OptionMeanA = question.OptionMeanA;
                        courseToUpdate.OptionB = question.OptionB;
                        courseToUpdate.OptionMeanB = question.OptionMeanB;
                        courseToUpdate.OptionC = question.OptionC;
                        courseToUpdate.OptionMeanC = question.OptionMeanC;
                        courseToUpdate.OptionD = question.OptionD;
                        courseToUpdate.OptionMeanD = question.OptionMeanD;
                        courseToUpdate.CorrectAnswer = question.CorrectAnswer;
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Question_List");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating Question: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the Question.");
                }
            }
            return View(question);
        }
        public async Task<IActionResult> Question_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Question ID.");
                TempData["ErrorMessage"] = "The specified Question was not found.";
                return RedirectToAction("Question_List", "Question");
            }

            var question = await _context.Questions.FirstOrDefaultAsync(c => c.QuestionId == id);

            // If no container is found, return to the list with an error
            if (question == null)
            {
                TempData["ErrorMessage"] = "The specified Question was not found.";
                return RedirectToAction("Question_List", "Question");
            }
            return View(question);
        }
    }
}
