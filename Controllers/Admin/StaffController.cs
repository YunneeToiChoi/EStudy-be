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

        // List all staff members
        public async Task<IActionResult> Staff_List()
        {
            var staffs = await _context.Staff.Include(s => s.Department).ToListAsync();
            return View(staffs);
        }

        // Display create form
        public async Task<IActionResult> Staff_Create()
        {
            var departments = await _context.Departments.ToListAsync();
            var roles = await _context.Roles.ToListAsync();
            var model = new StaffCreateViewModel
            {
                Staffs = new Staff(),
                Departments = departments.Select(c => new SelectListItem
                {
                    Value = c.DepartmentId.ToString(),
                    Text = c.DepartmentName
                }).ToList(),
                Roles = roles.Select(c => new SelectListItem
                {
                    Value = c.RoleId.ToString(),
                    Text = c.RoleName
                }).ToList()
            };
            return View(model);
        }

        // Handle create post
        [HttpPost]
        public async Task<IActionResult> Staff_Create(StaffCreateViewModel staffViewModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new staff.");
                staffViewModel.Departments = await _context.Departments.Select(c => new SelectListItem
                {
                    Value = c.DepartmentId.ToString(),
                    Text = c.DepartmentName
                }).ToListAsync();
                return View(staffViewModel);
            }

            var staff = new Staff
            {
                StaffName = staffViewModel.Staffs.StaffName,
                StaffType = staffViewModel.Staffs.StaffType,
                StaffCmnd = staffViewModel.Staffs.StaffCmnd,
                StaffEmail = staffViewModel.Staffs.StaffEmail,
                DepartmentId = staffViewModel.Staffs.DepartmentId,
                StaffPassword = staffViewModel.Staffs.StaffCmnd,
                RoleId = staffViewModel.Staffs.RoleId,
            };

            await _context.Staff.AddAsync(staff);
            await _context.SaveChangesAsync();
            return RedirectToAction("Staff_List");
        }

        // Display staff details
        public async Task<IActionResult> Staff_Details(int id)
        {
            var staff = await _context.Staff.Include(s => s.Department)
                                            .FirstOrDefaultAsync(s => s.StaffId == id);
            if (staff == null)
            {
                TempData["ErrorMessage"] = "Staff not found.";
                return RedirectToAction("Staff_List");
            }
            return View(staff);
        }

        public async Task<IActionResult> Staff_Edit(string id)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            var departments = await _context.Departments.ToListAsync();
            var model = new StaffEditViewModel
            {
                Staff = staff,
                Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.DepartmentName
                }).ToList()
            };
            return View(model);
        }

        // Handle edit post
        [HttpPost]
        public async Task<IActionResult> Staff_Edit(int id, StaffEditViewModel staffViewModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while updating staff.");
                return View(staffViewModel);
            }

            var staffToUpdate = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == id);
            if (staffToUpdate == null)
            {
                return NotFound();
            }

            staffToUpdate.StaffName = staffViewModel.Staff.StaffName;
            staffToUpdate.StaffEmail = staffViewModel.Staff.StaffEmail;
            staffToUpdate.StaffType = staffViewModel.Staff.StaffType;
            staffToUpdate.StaffCmnd = staffViewModel.Staff.StaffCmnd;
            staffToUpdate.DepartmentId = staffViewModel.Staff.DepartmentId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Staff_List");
        }

        // Display delete confirmation
        public async Task<IActionResult> Staff_Delete(int id)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == id);
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffCmnd == id);
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
                var courseToUpdate = await _context.Staff.FirstOrDefaultAsync(c => c.StaffCmnd == staff.StaffCmnd);
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
        public async Task<IActionResult> Staff_Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffCmnd == id);
            if (staff == null)
            {
                _logger.LogError($"Staff not found for delete.");
                return NotFound($"Staff not found.");
            }
            return View(staff);
        }

        [HttpPost, ActionName("Staff_Delete")]
        public async Task<IActionResult> Staff_DeleteConfirmed(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffCmnd == id);
            if (staff == null)
            {
                return NotFound();
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

        public async Task<IActionResult> Staff_Details(string id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Staff ID.");
                TempData["ErrorMessage"] = "The specified Staff was not found.";
                return RedirectToAction("Staff_List", "Staff");
            }

            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffCmnd == id);

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
