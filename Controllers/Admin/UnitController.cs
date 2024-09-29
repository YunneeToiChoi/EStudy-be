using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    public class UnitController : Controller
    {
        private readonly ILogger<UnitController> _logger;
        public UnitController(ILogger<UnitController> logger)
        {
            _logger = logger;
        }
        private readonly UnitRepository _unitsRepository = new UnitRepository();
        public Study4Context _context = new Study4Context();
        public async Task<IActionResult> Unit_List()
        {
            var units = await _context.Units.ToListAsync();
            return View(units);
        }
        public IActionResult Unit_Create()
        {
            var courses = _context.Courses.ToList();
            var model = new UnitCreateViewModel
            {
                Units = new Unit(),
                Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Unit_Create(UnitCreateViewModel unitViewModel)
        {

            try
            {
                var unit = new Unit
                {
                    UnitTittle = unitViewModel.Units.UnitTittle,
                    CourseId = unitViewModel.Units.CourseId
                    // map other properties if needed
                };

                await _context.AddAsync(unit);
                await _context.SaveChangesAsync();

                return RedirectToAction("Unit_List", "Unit");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                unitViewModel.Courses = _context.Courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                }).ToList();

                return View(unitViewModel);
            }
        }
        public async Task<IActionResult> GetUnitById(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }

            return Ok(unit);
        }
        [HttpGet]
        public IActionResult Unit_Edit(int id)
        {
            var unit = _context.Units.FirstOrDefault(c => c.UnitId == id);
            if (unit == null)
            {
                return NotFound();
            }
            return View(unit);
        }

        [HttpPost]
        public async Task<IActionResult> Unit_Edit(Unit unit)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Units.FirstOrDefault(c => c.UnitId == unit.UnitId);
                courseToUpdate.UnitTittle = unit.UnitTittle;
                courseToUpdate.Course = unit.Course;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Unit_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {unit.UnitId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(unit);
        }

        [HttpGet]
        public IActionResult Unit_Delete(int id)
        {
            var unit = _context.Units.FirstOrDefault(c => c.UnitId == id);
            if (unit == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(unit);
        }

        [HttpPost, ActionName("Unit_Delete")]
        public IActionResult Unit_DeleteConfirmed(int id)
        {
            var unit = _context.Units.FirstOrDefault(c => c.UnitId == id);
            if (unit == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Units.Remove(unit);
                _context.SaveChanges();
                return RedirectToAction("Unit_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(unit);
            }
        }

        public IActionResult Unit_Details(int id)
        {
            return View(_context.Units.FirstOrDefault(c => c.UnitId == id));
        }
    }
}
