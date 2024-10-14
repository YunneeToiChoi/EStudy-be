using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using study4_be.Models.ViewModel;
using study4_be.Models;
using Microsoft.EntityFrameworkCore;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using System.Numerics;

namespace study4_be.Controllers.Admin
{
    public class PlanCourseController : Controller
    {
        private readonly Study4Context _context;
        private readonly ILogger<LessonController> _logger;
        public PlanCourseController(Study4Context context, ILogger<LessonController> logger) 
        { 
            _context = context;
            _logger = logger;
        }
        
        public async Task<IActionResult> Plan_Course_List()
        {
            var planCourse = await _context.PlanCourses.ToListAsync();
            return View(planCourse);
        }
        public async Task<IActionResult> Plan_Course_Create()
        {
            var plan = await _context.Subscriptionplans.ToListAsync();

            var course = await _context.Courses.ToListAsync();

            var model = new PlanCourseCreateViewModel
            {
                planCourse = new PlanCourse(),

                plan = plan.Select(c => new SelectListItem
                {
                    Value = c.PlanId.ToString(),
                    Text = c.PlanName
                }).ToList(),

                course = course.Select(t => new SelectListItem
                {
                    Value = t.CourseId.ToString(),
                    Text = t.CourseName
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Plan_Course_Create(PlanCourseCreateViewModel planCourseCreate)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new plancourse.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                return View(planCourseCreate);
            }
            try
            {
                var planCourse = new PlanCourse
                {
                    PlanId = planCourseCreate.planCourse.PlanId,
                    CourseId = planCourseCreate.planCourse.CourseId,
                };

                await _context.AddAsync(planCourse);
                await _context.SaveChangesAsync();

                return RedirectToAction("Plan_Course_List", "PlanCourse");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new plancourse.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                return View(planCourseCreate);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Plan_Course_Delete(int planId, int courseId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"PlanCourse not found for deletion.");
                return NotFound($"PlanCourse not found.");
            }
            var planCourse = await _context.PlanCourses.FirstOrDefaultAsync(pc => pc.CourseId == courseId && pc.PlanId == planId);
            if (planCourse == null)
            {
                _logger.LogError($"PlanCourse not found for delete.");
                return NotFound($"PlanCourse not found.");
            }
            return View(planCourse);
        }

        [HttpPost, ActionName("Plan_Course_Delete")]
        public async Task<IActionResult> Plan_Course_DeleteConfirmed(int planId, int courseId)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"PlanCourse not found for deletion.");
                return NotFound($"PlanCourse not found.");
            }
            var planCourse = await _context.PlanCourses.FirstOrDefaultAsync(pc => pc.CourseId == courseId && pc.PlanId == planId);
            if (planCourse == null)
            {
                _logger.LogError($"PlanCourse not found for delete.");
                return NotFound($"PlanCourse not found.");
            }
            try
            {
                _context.PlanCourses.Remove(planCourse);
                await _context.SaveChangesAsync();
                return RedirectToAction("Plan_Course_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting PlanCourse");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the PlanCourse.");
                return View(planCourse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Plan_Course_Edit(int planId, int courseId)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Is invalid" });
            }

            var planCourse = await _context.PlanCourses.FirstOrDefaultAsync(pc => pc.CourseId == courseId && pc.PlanId == planId);
            if (planCourse == null) { return BadRequest(); }

            var plans = await _context.Subscriptionplans.ToListAsync();
            
            var selectListPlans = plans.Select(plan => new SelectListItem
            {
                Value = plan.PlanId.ToString(),
                Text = plan.PlanName
            }).ToList();

            var courses = await _context.Courses.ToListAsync();

            var selectListCourse = courses.Select(course => new SelectListItem
            {
                Value = course.CourseId.ToString(),
                Text = course.CourseName
            }).ToList();

            var viewModel = new PlanCourseCreateViewModel
            {
                planCourse = planCourse,
                oldCourseid = courseId,
                oldPlanid = planId,
                plan = selectListPlans,
                course = selectListCourse
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Plan_Course_Edit(PlanCourseCreateViewModel planCourseCreate)
        {
            if (ModelState.IsValid)
            {
                // Find the old PlanCourse entry using the original values
                var planCourseToDelete = await _context.PlanCourses
                    .FirstOrDefaultAsync(pc => pc.CourseId == planCourseCreate.oldCourseid && pc.PlanId == planCourseCreate.oldPlanid);

                if (planCourseToDelete != null)
                {
                    // Remove the old entry from the database
                    _context.PlanCourses.Remove(planCourseToDelete);
                }

                // Create a new PlanCourse entry with the updated values
                var newPlanCourse = new PlanCourse
                {
                    PlanId = planCourseCreate.planCourse.PlanId,
                    CourseId = planCourseCreate.planCourse.CourseId
                };

                // Add the new entry to the database
                _context.PlanCourses.Add(newPlanCourse);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Plan_Course_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating a new plan course");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the new plan course.");
                }
            }

            return View(planCourseCreate);
        }

        public async Task<IActionResult> Plan_Course_Details(int planId, int courseId)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid planCourse ID.");
                TempData["ErrorMessage"] = "The specified planCourse was not found.";
                return RedirectToAction("Plan_Course_List", "PlanCourse");
            }

            var planCourse = await _context.PlanCourses.FirstOrDefaultAsync(pc => pc.CourseId == courseId && pc.PlanId == planId);

            // If no container is found, return to the list with an error
            if (planCourse == null)
            {
                TempData["ErrorMessage"] = "The specified planCourse was not found.";
                return RedirectToAction("Plan_Course_List", "PlanCourse");
            }
            return View(planCourse);
        }
    }
}
