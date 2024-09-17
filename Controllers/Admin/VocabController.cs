using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services.Response;
using study4_be.Services;
using System.Diagnostics;

namespace study4_be.Controllers.Admin
{
    public class VocabController : Controller
    {
        private readonly ILogger<VocabController> _logger;
        private FireBaseServices _firebaseServices;
        private GeneralAiAudioServices _generalAiAudioServices;
        public VocabController(ILogger<VocabController> logger, FireBaseServices firebaseServices)
        {
            _logger = logger;
            _generalAiAudioServices = new GeneralAiAudioServices();
            _firebaseServices = firebaseServices;
        }
        private readonly VocabRepository _vocabsRepository = new VocabRepository();
        public Study4Context _context = new Study4Context();
        [HttpDelete("DeleteAllVocabs")]
        public async Task<IActionResult> DeleteAllVocabs()
        {
            await _vocabsRepository.DeleteAllVocabAsync();
            return Json(new { status = 200, message = "Delete Vocab Successful" });
        }
        public async Task<IActionResult> Vocab_List()
        {
            try
            {
                var vocabs = await _context.Vocabularies
                    .Include(v => v.Lesson)
                    .ThenInclude(l => l.Container)
                        .ThenInclude(c => c.Unit)
                            .ThenInclude(u => u.Course)
                    .ToListAsync();

                var vocabViewModels = vocabs
                    .Select(vocab => new VocabListViewModel
                    {
                        vocab = vocab,
                        courseName = vocab.Lesson?.Container?.Unit?.Course?.CourseName ?? "N/A",
                        unitTittle = vocab.Lesson?.Container?.Unit?.UnitTittle ?? "N/A",
                        containerTittle = vocab.Lesson?.Container?.ContainerTitle ?? "N/A",
                        lessonTittle = vocab.Lesson?.LessonTitle ?? "N/A",
                    }).ToList();

                return View(vocabViewModels);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error occurred while fetching vocabulary list.");

                // Handle the exception gracefully
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(new List<VocabListViewModel>());
            }
        }
        public async Task<IActionResult> Vocab_Create()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Container)
                    .ThenInclude(c => c.Unit)
                        .ThenInclude(u => u.Course)
                .ToListAsync();

            var model = new VocabCreateViewModel
            {
                vocab = new Vocabulary(),
                lesson = lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = $"{c.LessonTitle} - Container: {(c.Container?.ContainerTitle ?? "N/A")} - Unit: {(c.Container?.Unit?.UnitTittle ?? "N/A")} - Course: {(c.Container?.Unit?.Course?.CourseName ?? "N/A")} - TAG: {(c.TagId ?? "N/A")}"
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Vocab_Create(VocabCreateViewModel vocabViewModel)
        {
            var firebaseBucketName = _firebaseServices.GetFirebaseBucketName();
            // Generate and upload audio to Firebase Storage
            var uniqueId = Guid.NewGuid().ToString(); // Tạo một UUID ngẫu nhiên
            var audioFilePath = Path.Combine(Path.GetTempPath(), $"VOCAB({uniqueId}).wav");
            //var audioFilePath = Path.Combine(Path.GetTempPath(), $"VOCAB({vocabViewModel.vocab.VocabId}).wav");
            _generalAiAudioServices.GenerateAudio(vocabViewModel.vocab.VocabTitle, audioFilePath);

            var audioBytes = System.IO.File.ReadAllBytes(audioFilePath);
            var audioUrl = await _generalAiAudioServices.UploadFileToFirebaseStorageAsync(audioBytes, $"VOCAB({uniqueId}).wav", firebaseBucketName);
            // Delete the temporary file after uploading
            System.IO.File.Delete(audioFilePath);
            try
            {
                var vocabulary = new Vocabulary
                {
                    VocabId = vocabViewModel.vocab.VocabId,
                    VocabType = vocabViewModel.vocab.VocabType,
                    VocabTitle = vocabViewModel.vocab.VocabTitle,
                    AudioUrlUk = audioUrl,
                    AudioUrlUs = vocabViewModel.vocab.AudioUrlUs,
                    Mean = vocabViewModel.vocab.Mean,
                    Example = vocabViewModel.vocab.Example,
                    Explanation = vocabViewModel.vocab.Explanation,
                    LessonId = vocabViewModel.vocab.LessonId,
                };

                await _context.AddAsync(vocabulary);
                await _context.SaveChangesAsync();

                return RedirectToAction("Vocab_List", "Vocab");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                vocabViewModel.lesson = _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToList();

                return View(vocabViewModel);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVocabById(int id)
        {
            var vocab = await _context.Vocabularies.FindAsync(id);
            if (vocab == null)
            {
                return NotFound();
            }

            return Ok(vocab);
        }

        [HttpGet]
        public IActionResult Vocab_Delete(int id)
        {
            var vocab = _context.Vocabularies.FirstOrDefault(c => c.VocabId == id);
            if (vocab == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(vocab);
        }

        [HttpPost, ActionName("Vocab_Delete")]
        public IActionResult Vocab_DeleteConfirmed(int id)
        {
            var vocab = _context.Vocabularies.FirstOrDefault(c => c.VocabId == id);
            if (vocab == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Vocabularies.Remove(vocab);
                _context.SaveChanges();
                return RedirectToAction("Vocab_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(vocab);
            }
        }
        [HttpGet]
        public IActionResult Vocab_Edit(int id)
        {
            var vocab = _context.Vocabularies.FirstOrDefault(c => c.VocabId == id);
            if (vocab == null)
            {
                return NotFound();
            }
            return View(vocab);
        }

        [HttpPost]
        public async Task<IActionResult> Vocab_Edit(Vocabulary vocab)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Vocabularies.FirstOrDefault(c => c.VocabId == vocab.VocabId);
                courseToUpdate.VocabTitle = vocab.VocabTitle;
                courseToUpdate.VocabType = vocab.VocabType;
                courseToUpdate.Mean = vocab.Mean;
                courseToUpdate.Example = vocab.Example;
                courseToUpdate.Explanation = vocab.Explanation;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Vocab_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {vocab.VocabId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(vocab);
        }
        public IActionResult Vocab_Details(int id)
        {
            return View(_context.Vocabularies.FirstOrDefault(c => c.VocabId == id));
        }
    }
}
