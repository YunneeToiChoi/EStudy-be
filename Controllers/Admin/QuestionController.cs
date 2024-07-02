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
        public STUDY4Context _context = new STUDY4Context();
        //development enviroment
        [HttpDelete("DeleteAllQuestions")]
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
        public async Task<IActionResult> Question_Create(QuestionCreateViewModel questionViewModel, IFormFile QuestionImage)
        {
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
                    CorrectAnswer = questionViewModel.question.CorrectAnswer,
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
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                questionViewModel.lesson = _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToList();

                return View(questionViewModel);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuestionById(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            return Ok(question);
        }
        [HttpGet]
        public IActionResult Question_Delete(int id)
        {
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(question);
        }

        [HttpPost, ActionName("Question_Delete")]
        public IActionResult Question_DeleteConfirmed(int id)
        {
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Questions.Remove(question);
                _context.SaveChanges();
                return RedirectToAction("Course_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(question);
            }
        }
        [HttpGet]
        public IActionResult Question_Edit(int id)
        {
            var question = _context.Questions.FirstOrDefault(c => c.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> Question_Edit(Question question, IFormFile QuestionImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var courseToUpdate = _context.Questions.FirstOrDefault(c => c.QuestionId == question.QuestionId);
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
                        courseToUpdate.QuestionTextMean = question.QuestionText;
                        courseToUpdate.QuestionParagraph = question.QuestionParagraph;
                        courseToUpdate.QuestionParagraphMean = question.QuestionParagraphMean;
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
                        return View(question);
                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {question.QuestionId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(question);
        }
        public IActionResult Question_Details(int id)
        {
            return View(_context.Questions.FirstOrDefault(c => c.QuestionId == id));
        }
    }
}
