using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class LessonController : Controller
    {
        private readonly ILogger<LessonController> _logger;
        public LessonController(ILogger<LessonController> logger)
        {
            _logger = logger;
        }
        private readonly LessonRepository _lessonsRepository = new LessonRepository();
        public STUDY4Context _context = new STUDY4Context();
        [HttpGet("GetAllLessons")]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetAllLessons()
        {
            var lessons = await _lessonsRepository.GetAllLessonsAsync();
            return Json(new { status = 200, message = "Get Lessons Successful", lessons });

        }

        [HttpDelete("DeleteAllLessons")]
        public async Task<IActionResult> DeleteAllLessons()
        {
            await _lessonsRepository.DeleteAllLessonsAsync();
            return Json(new { status = 200, message = "Delete Lessons Successful" });
        }
        public IActionResult Lesson_List()
        {
            return View();
        }
        public IActionResult Lesson_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Lesson_Create(Lesson lesson)
        {
            if (!ModelState.IsValid)
            {

                return View(lesson);    //show form with value input and show errors
            }
            try
            {
                try
                {
                    await _context.AddAsync(lesson);
                    await _context.SaveChangesAsync();
                    CreatedAtAction(nameof(GetLessonById), new { id = lesson.LessonId }, lesson);
                }
                catch (Exception e)
                {
                    CreatedAtAction(nameof(GetLessonById), new { id = lesson.LessonId }, lesson);
                    _logger.LogError(e, "Error occurred while creating new lesson.");
                }
                return RedirectToAction("Index", "Home"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new lesson.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(lesson);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLessonById(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            return Ok(lesson);
        }

        public IActionResult Lesson_Delete()
        {
            return View();
        }
        public IActionResult Lesson_Edit()
        {
            return View();
        }
        public IActionResult Lesson_Details()
        {
            return View();
        }

    }
}
