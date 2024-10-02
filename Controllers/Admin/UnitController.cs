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
        private readonly UnitRepository _unitsRepository;
        private  readonly Study4Context _context;
        public UnitController(ILogger<UnitController> logger, Study4Context context)
        {
            _logger = logger;
            _context = context;
            _unitsRepository = new(context);
        }
        
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
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                unitViewModel.Courses = _context.Courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                }).ToList();

                return View(unitViewModel);
            }
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
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }

            return Ok(unit);
        }
        [HttpGet]
        public async Task<IActionResult> Unit_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "unitId is invalid" });
            }
            var unit = await _context.Units.FirstOrDefaultAsync(c => c.UnitId == id);
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
                var courseToUpdate = await _context.Units.FirstOrDefaultAsync(c => c.UnitId == unit.UnitId);
                courseToUpdate.UnitTittle = unit.UnitTittle;
                courseToUpdate.Course = unit.Course;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Unit_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating unit: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the unit.");
                }
            }
            return View(unit);
        }

        [HttpGet]
        public async Task<IActionResult> Unit_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Unit not found for deletion.");
                return NotFound($"Unit not found.");
            }
            var unit = await _context.Units.FirstOrDefaultAsync(c => c.UnitId == id);
            if (unit == null)
            {
                _logger.LogError($"Unit not found for deletion.");
                return NotFound($"Unit not found.");
            }
            return View(unit);
        }

        [HttpPost, ActionName("Unit_Delete")]
        public async Task<IActionResult> Unit_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Unit not found for deletion.");
                return NotFound($"Unit not found.");
            }
            var unit = await _context.Units.FirstOrDefaultAsync(c => c.UnitId == id);
            if (unit == null)
            {
                _logger.LogError($"Unit not found for deletion.");
                return NotFound($"Unit not found.");
            }

            try
            {
                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();
                return RedirectToAction("Unit_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting unit: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the unit.");
                return View(unit);
            }
        }

        public async Task<IActionResult> Unit_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Unit ID.");
                TempData["ErrorMessage"] = "The specified Unit was not found.";
                return RedirectToAction("Unit_List", "Unit");
            }

            var unit = await _context.Units.FirstOrDefaultAsync(c => c.UnitId == id);

            // If no container is found, return to the list with an error
            if (unit == null)
            {
                TempData["ErrorMessage"] = "The specified unit was not found.";
                return RedirectToAction("Unit_List", "Unit");
            }
            return View(unit);
        }
    }
}
