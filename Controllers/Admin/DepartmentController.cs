using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("GetAllDepartments")]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _departmentsRepository.GetAllDepartmentsAsync();
            return Json(new { status = 200, message = "Get Department Successful", departments });

        }
        //development enviroment
        [HttpDelete("DeleteAllDepartments")]
        public async Task<IActionResult> DeleteAllDepartments()
        {
            await _departmentsRepository.DeleteAllDepartmentsAsync();
            return Json(new { status = 200, message = "Delete Department Successful" });
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
                _logger.LogError(ex, "Error occurred while creating new department.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(department);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {

            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
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
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "departmentId is invalid" });
            }
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
                var courseToUpdate = await _context.Departments.FirstOrDefaultAsync(c => c.DepartmentId == department.DepartmentId);
                courseToUpdate.DepartmentName = department.DepartmentName;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Department_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {department.DepartmentId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the department.");
                }
            }
            return View(department);
        }
        [HttpGet]
        public IActionResult Department_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Department with ID {id} not found for deletion.");
                return NotFound($"Department with ID {id} not found.");
            }
            var department = _context.Departments.FirstOrDefault(c => c.DepartmentId == id);
            if (department == null)
            {
                _logger.LogError($"Department with ID {id} not found for delete.");
                return NotFound($"Department with ID {id} not found.");
            }
            return View(department);
        }

        [HttpPost, ActionName("Department_Delete")]
        public IActionResult Department_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Department with ID {id} not found for deletion.");
                return NotFound($"Department with ID {id} not found.");
            }

            var department = _context.Departments.FirstOrDefault(c => c.DepartmentId == id);
            if (department == null)
            {
                _logger.LogError($"Department with ID {id} not found for deletion.");
                return NotFound($"Department with ID {id} not found.");
            }

            try
            {
                _context.Departments.Remove(department);
                _context.SaveChanges();
                return RedirectToAction("Department_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting department with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the department.");
                return View(department);
            }
        }

        public IActionResult Department_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid department ID.");
                TempData["ErrorMessage"] = "The specified department was not found.";
                return RedirectToAction("Department_List", "Department");
            }

            var department = _context.Departments.FirstOrDefault(c => c.DepartmentId == id);

            // If no container is found, return to the list with an error
            if (department == null)
            {
                TempData["ErrorMessage"] = "The specified department was not found.";
                return RedirectToAction("Department_List", "Department");
            }
            return View(department);
        }
    }
}
