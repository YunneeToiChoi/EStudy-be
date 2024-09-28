using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    public class DepartmentController : Controller
    {
        private readonly ILogger<DepartmentController> _logger;
        public DepartmentController(ILogger<DepartmentController> logger)
        {
            _logger = logger;
        }
        private readonly DepartmentRepository _departmentsRepository = new DepartmentRepository();
        public Study4Context _context = new Study4Context();

        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _departmentsRepository.GetAllDepartmentsAsync();
            return Json(new { status = 200, message = "Get Courses Successful", departments });

        }
        public async Task<IActionResult> DeleteAllDepartments()
        {
            await _departmentsRepository.DeleteAllDepartmentsAsync();
            return Json(new { status = 200, message = "Delete Courses Successful" });
        }
        public async Task<IActionResult> Department_List()
        {
            var departments = await _departmentsRepository.GetAllDepartmentsAsync();
            return View(departments);
        }
        public IActionResult Department_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Department_Create(Department department)
        {
            if (!ModelState.IsValid)
            {

                return View(department);
            }
            try
            {
                await _context.AddAsync(department);
                await _context.SaveChangesAsync();
                CreatedAtAction(nameof(GetDepartmentById), new { id = department.DepartmentId }, department);
                return RedirectToAction("Department_List", "Department"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new course.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(department);
            }
        }
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return Ok(department);
        }

        [HttpGet]
        public IActionResult Department_Edit(int id)
        {
            var department = _context.Departments.FirstOrDefault(c => c.DepartmentId == id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }
        [HttpPost]
        public async Task<IActionResult> Department_Edit(Department department)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Departments.FirstOrDefault(c => c.DepartmentId == department.DepartmentId);
                courseToUpdate.DepartmentName = department.DepartmentName;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Department_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {department.DepartmentId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(department);
        }
        [HttpGet]
        public IActionResult Department_Delete(int id)
        {
            var department = _context.Departments.FirstOrDefault(c => c.DepartmentId == id);
            if (department == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(department);
        }

        [HttpPost, ActionName("Department_Delete")]
        public IActionResult Department_DeleteConfirmed(int id)
        {
            var department = _context.Departments.FirstOrDefault(c => c.DepartmentId == id);
            if (department == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Departments.Remove(department);
                _context.SaveChanges();
                return RedirectToAction("Department_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(department);
            }
        }

        public IActionResult Department_Details(int id)
        {
            return View(_context.Departments.FirstOrDefault(c => c.DepartmentId == id));
        }
    }
}
