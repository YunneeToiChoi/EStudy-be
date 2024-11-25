
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    [Route("Admin/HR/[controller]/[action]")]
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
        public async Task<IActionResult> Staff_Details(string id)
        {
            var staff = await _context.Staff.Include(s => s.Department)
                                            .FirstOrDefaultAsync(s => s.StaffCmnd == id);
            if (staff == null)
            {
                TempData["ErrorMessage"] = "Staff not found.";
                return RedirectToAction("Staff_List");
            }
            return View(staff);
        }

        public async Task<IActionResult> Staff_Edit(string id)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffCmnd == id);
            if (staff == null)
            {
                return NotFound();
            }

            var departments = await _context.Departments.ToListAsync();
            var roles = await _context.Roles.ToListAsync();
            var model = new StaffEditViewModel
            {
                Staffs = staff,
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

        // Handle edit post
        [HttpPost]
        public async Task<IActionResult> Staff_Edit(string id, StaffEditViewModel staffViewModel)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while updating staff.");
                return View(staffViewModel);
            }

            var staffToUpdate = await _context.Staff.FirstOrDefaultAsync(s => s.StaffCmnd == id);
            if (staffToUpdate == null)
            {
                return NotFound();
            }

            staffToUpdate.StaffName = staffViewModel.Staffs.StaffName;
            staffToUpdate.StaffEmail = staffViewModel.Staffs.StaffEmail;
            staffToUpdate.StaffType = staffViewModel.Staffs.StaffType;
            staffToUpdate.StaffCmnd = staffViewModel.Staffs.StaffCmnd;
            staffToUpdate.DepartmentId = staffViewModel.Staffs.DepartmentId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Staff_List");
        }

        [HttpGet]
        // Display delete confirmation
        public async Task<IActionResult> Staff_Delete(string id)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(c => c.StaffCmnd == id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }
        [HttpPost]
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
    }
}
