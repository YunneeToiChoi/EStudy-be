using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;

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
        public async Task<IActionResult> Lesson_List()
        {
            var container = await _context.Containers
              .Include(c => c.Unit)
                  .ThenInclude(u => u.Course)
              .ToListAsync();
            var lesson = await _context.Lessons.ToListAsync();

            var lessonViewModels = lesson.Select(lesson => new LessonListViewModel
            {
                Lesson = lesson,
                containerTitle = container.FirstOrDefault(c => c.ContainerId == lesson.ContainerId)?.ContainerTitle ?? "N/A",
                unitTitle = container.FirstOrDefault(c => c.ContainerId == lesson.ContainerId)?.Unit?.UnitTittle ?? "N/A",
                courseTitle = container.FirstOrDefault(c => c.ContainerId == lesson.ContainerId)?.Unit?.Course?.CourseName ?? "N/A"
            });


            return View(lessonViewModels);
        }
        public IActionResult Lesson_Create()
        {
            var containers = _context.Containers
                .Include(c => c.Unit)
                    .ThenInclude(u => u.Course)
                .ToList();

            var tags = _context.Tags.ToList();

            var model = new LessonCreateViewModel
            {
                lesson = new Lesson(),
                container = containers.Select(c => new SelectListItem
                {
                    Value = c.ContainerId.ToString(),
                    Text = $"{c.ContainerTitle} : {c.Unit.UnitTittle} : {c.Unit.Course.CourseName}"
                }).ToList(),
                tag = tags.Select(t => new SelectListItem
                {
                    Value = t.TagId.ToString(),
                    Text = t.TagId
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Lesson_Create(LessonCreateViewModel lessonViewModel)
        {
            try
            {
                var lesson = new Lesson
                {
                    LessonType = lessonViewModel.lesson.LessonType,
                    LessonId = lessonViewModel.lesson.LessonId,
                    LessonTitle = lessonViewModel.lesson.LessonTitle,
                    ContainerId = lessonViewModel.lesson.ContainerId,
                    TagId = lessonViewModel.lesson.TagId,

                    // map other properties if needed
                };

                await _context.AddAsync(lesson);
                await _context.SaveChangesAsync();

                return RedirectToAction("Lesson_List", "Lesson");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                lessonViewModel.container = _context.Containers.Select(c => new SelectListItem
                {
                    Value = c.ContainerId.ToString(),
                    Text = c.ContainerId.ToString()
                }).ToList();

                return View(lessonViewModel);
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

        [HttpGet]
        public IActionResult Lesson_Delete(int id)
        {
            var lesson = _context.Lessons.FirstOrDefault(c => c.LessonId == id);
            if (lesson == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(lesson);
        }

        [HttpPost, ActionName("Lesson_Delete")]
        public IActionResult Lesson_DeleteConfirmed(int id)
        {
            var lesson = _context.Lessons.FirstOrDefault(c => c.LessonId == id);
            if (lesson == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Lessons.Remove(lesson);
                _context.SaveChanges();
                return RedirectToAction("Lesson_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(lesson);
            }
        }
        [HttpGet]
        public IActionResult Lesson_Edit(int id)
        {
            var lesson = _context.Lessons.FirstOrDefault(c => c.LessonId == id);
            if (lesson == null)
            {
                return NotFound();
            }
            return View(lesson);
        }

        [HttpPost]
        public async Task<IActionResult> Course_Edit(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Lessons.FirstOrDefault(c => c.LessonId == lesson.LessonId);
                courseToUpdate.LessonTitle = lesson.LessonTitle;
                courseToUpdate.LessonType = lesson.LessonType;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Lesson_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {lesson.LessonId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(lesson);
        }
        public IActionResult Lesson_Details(int id)
        {
            return View(_context.Lessons.FirstOrDefault(c => c.LessonId == id));
        }
    }
}
