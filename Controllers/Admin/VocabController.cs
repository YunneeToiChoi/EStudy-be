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
        private readonly VocabRepository _vocabsRepository;
        private readonly Study4Context _context;
        public VocabController(ILogger<VocabController> logger, FireBaseServices firebaseServices, Study4Context context)
        {
            _logger = logger;
            _generalAiAudioServices = new GeneralAiAudioServices();
            _firebaseServices = firebaseServices;
            _context = context;
            _vocabsRepository = new(context);
        }
        
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
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new vocab.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                vocabViewModel.lesson = await _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToListAsync();

                return View(vocabViewModel);
            }
            var firebaseBucketName = _firebaseServices.GetFirebaseBucketName();
            // Generate and upload audio to Firebase Storage
            var uniqueId = Guid.NewGuid().ToString(); // Tạo một UUID ngẫu nhiên
            var audioFilePath = Path.Combine(Path.GetTempPath(), $"VOCAB({uniqueId}).wav");
            //var audioFilePath = Path.Combine(Path.GetTempPath(), $"VOCAB({vocabViewModel.vocab.VocabId}).wav");
            _generalAiAudioServices.GenerateAudio(vocabViewModel.vocab.VocabTitle, audioFilePath);

            var audioBytes = await System.IO.File.ReadAllBytesAsync(audioFilePath);
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
                _logger.LogError(ex, "Error occurred while creating new vocab.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                vocabViewModel.lesson = await _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToListAsync();

                return View(vocabViewModel);
            }
        }

        public async Task<IActionResult> GetVocabById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var vocab = await _context.Vocabularies.FindAsync(id);
            if (vocab == null)
            {
                return NotFound();
            }

            return Ok(vocab);
        }

        [HttpGet]
        public async Task<IActionResult> Vocab_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Vocab not found for deletion.");
                return NotFound($"Vocab not found.");
            }
            var vocab = await _context.Vocabularies.FirstOrDefaultAsync(c => c.VocabId == id);
            if (vocab == null)
            {
                _logger.LogError($"Vocab with ID {id} not found for delete.");
                return NotFound($"Vocab with ID {id} not found.");
            }
            return View(vocab);
        }

        [HttpPost, ActionName("Vocab_Delete")]
        public async Task<IActionResult> Vocab_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Vocab not found for deletion.");
                return NotFound($"Vocab not found.");
            }
            var vocab = await _context.Vocabularies.FirstOrDefaultAsync(c => c.VocabId == id);
            if (vocab == null)
            {
                _logger.LogError($"Vocab not found for deletion.");
                return NotFound($"Vocab not found.");
            }
            try
            {
                _context.Vocabularies.Remove(vocab);
                await _context.SaveChangesAsync();
                return RedirectToAction("Vocab_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting vocab: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the vocab.");
                return View(vocab);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Vocab_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var vocab = await _context.Vocabularies.FirstOrDefaultAsync(c => c.VocabId == id);
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
                var courseToUpdate = await _context.Vocabularies.FirstOrDefaultAsync(c => c.VocabId == vocab.VocabId);
                courseToUpdate.VocabTitle = vocab.VocabTitle;
                courseToUpdate.VocabType = vocab.VocabType;
                courseToUpdate.Mean = vocab.Mean;
                courseToUpdate.Example = vocab.Example;
                courseToUpdate.Explanation = vocab.Explanation;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Vocab_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating vocab: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the vocab.");
                }
            }
            return View(vocab);
        }
        public async Task<IActionResult> Vocab_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Vocab ID.");
                TempData["ErrorMessage"] = "The specified Vocab was not found.";
                return RedirectToAction("Vocab_List", "Vocab");
            }

            var vocab = await _context.Vocabularies.FirstOrDefaultAsync(c => c.VocabId == id);

            // If no container is found, return to the list with an error
            if (vocab == null)
            {
                TempData["ErrorMessage"] = "The specified vocab was not found.";
                return RedirectToAction("Vocab_List", "Vocab");
            }
            return View(vocab);
        }
    }
}
