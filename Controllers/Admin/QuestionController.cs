﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class QuestionController : Controller
    {
        private readonly ILogger<QuestionController> _logger;
        public QuestionController(ILogger<QuestionController> logger)
        {
            _logger = logger;
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
        public async Task<IActionResult> Question_Create(QuestionCreateViewModel questionViewModel)
        {
            try
            {
                var question = new Question
                {
                    QuestionId = questionViewModel.question.QuestionId,
                    QuestionText = questionViewModel.question.QuestionText,
                    CorrectAnswer = questionViewModel.question.CorrectAnswer,
                    OptionA = questionViewModel.question.OptionA,
                    OptionB = questionViewModel.question.OptionB,
                    OptionC= questionViewModel.question.OptionC,
                    OptionD= questionViewModel.question.OptionD,
                    LessonId= questionViewModel.question.LessonId,
                };

                await _context.AddAsync(question);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
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
        public IActionResult Question_Delete()
        {
            return View();
        }
        public IActionResult Question_Edit()
        {
            return View();
        }
        public IActionResult Question_Details()
        {
            return View();
        }
    }
}
