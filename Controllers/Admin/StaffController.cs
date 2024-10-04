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
        private readonly Study4Context _context;
        public StaffController(ILogger<StaffController> logger, Study4Context context)
        {
            _logger = logger;
            _context = context;
        }
        
        public async Task<IActionResult> Staff_List()
        {
            var staffs = await _context.Staff.ToListAsync();
            return View(staffs);
        }
        public async Task<IActionResult> Staff_Create()
        {
            var departments = await _context.Departments.ToListAsync();
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
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new staff.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                staffViewModel.Departments = await _context.Departments.Select(c => new SelectListItem
                {
                    Value = c.DepartmentId.ToString(),
                    Text = c.DepartmentName
                }).ToListAsync();

                return View(staffViewModel);
            }
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
                _logger.LogError(ex, "Error occurred while creating new staff.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                staffViewModel.Departments = await _context.Departments.Select(c => new SelectListItem
                {
                    Value = c.DepartmentId.ToString(),
                    Text = c.DepartmentName
                }).ToListAsync();

                return View(staffViewModel);
            }
        }

        public async Task<IActionResult> GetStaffById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var staff = await _context.Staff.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }

            return Ok(staff);
        }

        public async Task<IActionResult> Staff_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "staffId is invalid" });
            }
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffId == id);
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
                var courseToUpdate = await _context.Staff.FirstOrDefaultAsync(c => c.StaffId == staff.StaffId);
                courseToUpdate.StaffName = staff.StaffName;
                courseToUpdate.StaffEmail = staff.StaffEmail;
                courseToUpdate.StaffType = staff.StaffName;
                courseToUpdate.StaffCmnd = staff.StaffCmnd;
                courseToUpdate.Department = staff.Department;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Staff_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating staff ");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the staff.");
                }
            }
            return View(staff);
        }

        [HttpGet]
        public async Task<IActionResult> Staff_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffId == id);
            if (staff == null)
            {
                _logger.LogError($"Staff not found for delete.");
                return NotFound($"Staff not found.");
            }
            return View(staff);
        }

        [HttpPost, ActionName("Staff_Delete")]
        public async Task<IActionResult> Staff_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffId == id);
            if (staff == null)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }

            try
            {
                _context.Staff.Remove(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction("Staff_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the staff.");
                return View(staff);
            }
        }

        public async Task<IActionResult> Staff_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Staff ID.");
                TempData["ErrorMessage"] = "The specified Staff was not found.";
                return RedirectToAction("Staff_List", "Staff");
            }

            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffId == id);

            // If no container is found, return to the list with an error
            if (staff == null)
            {
                TempData["ErrorMessage"] = "The specified Staff was not found.";
                return RedirectToAction("Staff_List", "Staff");
            }
            return View(staff);
        }
    }
}
