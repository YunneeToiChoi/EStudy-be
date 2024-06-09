﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class VocabController : Controller
    {
        private readonly ILogger<VocabController> _logger;
        public VocabController(ILogger<VocabController> logger)
        {
            _logger = logger;
        }
        private readonly VocabRepository _vocabsRepository = new VocabRepository();
        public STUDY4Context _context = new STUDY4Context();
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
                    Text = $"{c.LessonTitle} - Container: {(c.Container?.ContainerTitle ?? "N/A")} - Unit: {(c.Container?.Unit?.UnitTittle ?? "N/A")} - Course: {(c.Container?.Unit?.Course?.CourseName ?? "N/A")}"
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Vocab_Create(VocabCreateViewModel vocabViewModel)
        {
            try
            {
                var vocabulary = new Vocabulary
                {
                    VocabId = vocabViewModel.vocab.VocabId,
                    VocabType = vocabViewModel.vocab.VocabType,
                    VocabTitle = vocabViewModel.vocab.VocabTitle,
                    AudioUrlUk = vocabViewModel.vocab.AudioUrlUk,
                    AudioUrlUs = vocabViewModel.vocab.AudioUrlUs,
                    Mean = vocabViewModel.vocab.Mean,
                    Example = vocabViewModel.vocab.Example,
                    Explanation = vocabViewModel.vocab.Explanation,
                    LessonId = vocabViewModel.vocab.LessonId,
                };

                await _context.AddAsync(vocabulary);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
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

        public IActionResult Vocab_Delete()
        {
            return View();
        }
        public IActionResult Vocab_Edit()
        {
            return View();
        }
        public IActionResult Vocab_Details()
        {
            return View();
        }
    }
}
