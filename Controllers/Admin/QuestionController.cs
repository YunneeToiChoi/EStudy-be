using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
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

        [HttpGet("GetAllQuestions")]
        public async Task<ActionResult<IEnumerable<Question>>> GetAllQuestions()
        {
            var questions = await _questionsRepository.GetAllQuestionsAsync();
            return Json(new { status = 200, message = "Get Questions Successful", questions });

        }
        //development enviroment
        [HttpDelete("DeleteAllQuestions")]
        public async Task<IActionResult> DeleteAllQuestions()
        {
            await _questionsRepository.DeleteAllQuestionsAsync();
            return Json(new { status = 200, message = "Delete Questions Successful" });
        }
        public IActionResult Question_List()
        {
            return View();
        }
        public IActionResult Question_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Question_Create(Question question)
        {
            if (!ModelState.IsValid)
            {

                return View(question);    //show form with value input and show errors
            }
            try
            {
                try
                {
                    await _context.AddAsync(question);
                    await _context.SaveChangesAsync();
                    CreatedAtAction(nameof(GetQuestionById), new { id = question.QuestionId }, question);
                }
                catch (Exception e)
                {
                    CreatedAtAction(nameof(GetQuestionById), new { id = question.QuestionId }, question);
                    _logger.LogError(e, "Error occurred while creating new question.");
                }
                return RedirectToAction("Index", "Home"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new question.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(question);
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
