using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    [Route("Admin/HR/[controller]/[action]")]
    public class DepartmentController : Controller
    {
        private readonly ILogger<DepartmentController> _logger;
        private readonly DepartmentRepository _departmentsRepository;
        private readonly Study4Context _context;
        public DepartmentController(ILogger<DepartmentController> logger, Study4Context context)
        {
            _logger = logger;
            _context = context;
            _departmentsRepository = new(context);
        }
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _departmentsRepository.GetAllDepartmentsAsync();
            return Json(new { status = 200, message = "Get Department Successful", departments });

        }
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
        public async Task<IActionResult> Department_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "departmentId is invalid" });
            }
            var department = await _context.Departments.FirstOrDefaultAsync(c => c.DepartmentId == id);
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
                var departmentToUpdate = await _context.Departments.FirstOrDefaultAsync(c => c.DepartmentId == department.DepartmentId);
                departmentToUpdate.DepartmentName = department.DepartmentName;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Department_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating course");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the department.");
                }
            }
            return View(department);
        }
        [HttpGet]
        public async Task<IActionResult> Department_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Department not found for deletion.");
                return NotFound($"Department not found.");
            }
            var department = await _context.Departments.FirstOrDefaultAsync(c => c.DepartmentId == id);
            if (department == null)
            {
                _logger.LogError($"Department not found for delete.");
                return NotFound($"Department not found.");
            }
            return View(department);
        }

        [HttpPost, ActionName("Department_Delete")]
        public async Task<IActionResult> Department_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Department not found for deletion.");
                return NotFound($"Department not found.");
            }

            var department = await _context.Departments.FirstOrDefaultAsync(c => c.DepartmentId == id);
            if (department == null)
            {
                _logger.LogError($"Department not found for deletion.");
                return NotFound($"Department not found.");
            }

            try
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                return RedirectToAction("Department_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the department.");
                return View(department);
            }
        }

        public async Task<IActionResult> Department_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid department ID.");
                TempData["ErrorMessage"] = "The specified department was not found.";
                return RedirectToAction("Department_List", "Department");
            }

            var department = await _context.Departments.FirstOrDefaultAsync(c => c.DepartmentId == id);

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
