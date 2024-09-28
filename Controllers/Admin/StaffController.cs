using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class StaffController : Controller
    {
        private readonly ILogger<StaffController> _logger;
        public StaffController(ILogger<StaffController> logger)
        {
            _logger = logger;
        }
        private readonly StaffRepository _staffsRepository = new StaffRepository();
        private Study4Context _context = new Study4Context();
        public async Task<IActionResult> Staff_List()
        {
            var staffs = await _context.Staff.ToListAsync();
            return View(staffs);
        }
        public IActionResult Staff_Create()
        {
            var departments = _context.Departments.ToList();
            var model = new StaffCreateViewModel
            {
                Staffs = new Staff(),
                Departments = departments.Select(c => new SelectListItem
                {
                    Value = c.DepartmentId.ToString(),
                    Text = c.DepartmentName
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Staff_Create(StaffCreateViewModel staffViewModel)
        {

            try
            {
                var staff = new Staff
                {
                    StaffName = staffViewModel.Staffs.StaffName,
                    StaffType = staffViewModel.Staffs.StaffType,
                    StaffCmnd = staffViewModel.Staffs.StaffCmnd,
                    StaffEmail = staffViewModel.Staffs.StaffEmail,
                    DepartmentId = staffViewModel.Staffs.DepartmentId
                    // map other properties if needed
                };

                await _context.AddAsync(staff);
                await _context.SaveChangesAsync();

                return RedirectToAction("Staff_List", "Staff");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                staffViewModel.Departments = _context.Departments.Select(c => new SelectListItem
                {
                    Value = c.DepartmentId.ToString(),
                    Text = c.DepartmentName
                }).ToList();

                return View(staffViewModel);
            }
        }

        public async Task<IActionResult> GetStaffById(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            return Ok(staff);
        }

        public IActionResult Staff_Edit(int id)
        {
            var staff = _context.Staff.FirstOrDefault(c => c.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Staff_Edit(Staff staff)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Staff.FirstOrDefault(c => c.StaffId == staff.StaffId);
                courseToUpdate.StaffName = staff.StaffName;
                courseToUpdate.StaffEmail = staff.StaffEmail;
                courseToUpdate.StaffType = staff.StaffName;
                courseToUpdate.StaffCmnd = staff.StaffCmnd;
                courseToUpdate.Department = staff.Department;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Staff_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {staff.StaffId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(staff);
        }

        [HttpGet]
        public IActionResult Staff_Delete(int id)
        {
            var staff = _context.Staff.FirstOrDefault(c => c.StaffId == id);
            if (staff == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(staff);
        }

        [HttpPost, ActionName("Staff_Delete")]
        public IActionResult Staff_DeleteConfirmed(int id)
        {
            var staff = _context.Staff.FirstOrDefault(c => c.StaffId == id);
            if (staff == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Staff.Remove(staff);
                _context.SaveChanges();
                return RedirectToAction("Staff_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(staff);
            }
        }

        public IActionResult Staff_Details(int id)
        {
            return View(_context.Staff.FirstOrDefault(c => c.StaffId == id));
        }
    }
}
