using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace study4_be.Controllers.Admin
{
    public class ContainerController : Controller
    {
        private readonly ILogger<ContainerController> _logger;
        public ContainerController(ILogger<ContainerController> logger)
        {
            _logger = logger;
        }
        private readonly ContainerRepository _containersRepository = new ContainerRepository();
        public STUDY4Context _context = new STUDY4Context();
        public async Task<IActionResult> Container_List()
        {
            var containers = await _context.Containers
                   .Include(c => c.Unit)
                       .ThenInclude(u => u.Course)
                   .ToListAsync();

            var containerViewModels = containers.Select(container => new ContainerListViewModel
            {
                container = container,
                courseName = $"{container.Unit.Course.CourseName}",
                unitTitle = container.Unit.UnitTittle // Assuming UnitTittle is the property for the unit title
            });

            return View(containerViewModels);
        }
        public IActionResult Container_Create()
        {
            var units = _context.Units.Include(u => u.Course).ToList();
            var model = new ContainerCreateViewModel
            {
                containers = new Container(),
                units = units.Select(c => new SelectListItem
                {
                    Value = c.UnitId.ToString(),
                    Text = c.UnitTittle + " : " + c.Course.CourseName
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Container_Create(ContainerCreateViewModel containerViewModel)
        {
            _logger.LogInformation("ContainerTitle: {ContainerTitle}", containerViewModel.containers.ContainerTitle);
            _logger.LogInformation("UnitId: {UnitId}", containerViewModel.containers.UnitId);
            //if (!ModelState.IsValid)
            //{
            //    // Debug: Kiểm tra các lỗi trong ModelState
            //    foreach (var modelState in ModelState)
            //    {
            //        var key = modelState.Key;
            //        var errors = modelState.Value.Errors;
            //        foreach (var error in errors)
            //        {
            //            _logger.LogWarning($"Error in key '{key}': {error.ErrorMessage}");
            //        }
            //    }

            //    // Nạp lại danh sách units nếu có lỗi
            //    containerViewModel.units = _context.Units.Select(c => new SelectListItem
            //    {
            //        Value = c.UnitId.ToString(),
            //        Text = c.UnitTittle + " : " + c.Course.CourseName
            //    }).ToList();

            //    return View(containerViewModel);
            //}

            try
            {
                int? unitId = string.IsNullOrEmpty(containerViewModel.containers.UnitId.ToString()) ? (int?)null : containerViewModel.containers.UnitId;

                var container = new Container
                {
                    ContainerId = containerViewModel.containers.ContainerId,
                    ContainerTitle = containerViewModel.containers.ContainerTitle,
                    UnitId = unitId
                };

                await _context.AddAsync(container);
                await _context.SaveChangesAsync();

                return RedirectToAction("Container_List", "Container");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new container.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                // Nạp lại danh sách units nếu có lỗi
                containerViewModel.units = _context.Units.Select(c => new SelectListItem
                {
                    Value = c.UnitId.ToString(),
                    Text = c.UnitTittle + " : " + c.Course.CourseName
                }).ToList();

                return View(containerViewModel);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetContainerById(int id)
        {
            var container = await _context.Containers.FindAsync(id);
            if (container == null)
            {
                return NotFound();
            }

            return Ok(container);
        }

        [HttpGet]
        public IActionResult Container_Edit(int id)
        {
            var container = _context.Containers.FirstOrDefault(c => c.ContainerId == id);
            if (container == null)
            {
                return NotFound();
            }
            return View(container);
        }

        [HttpPost]
        public async Task<IActionResult> Container_Edit(Container container)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Containers.FirstOrDefault(c => c.ContainerId == container.ContainerId);
                courseToUpdate.ContainerTitle = container.ContainerTitle;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Container_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {container.ContainerId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(container);
        }

        [HttpGet]
        public IActionResult Container_Delete(int id)
        {
            var container = _context.Containers.FirstOrDefault(c => c.ContainerId == id);
            if (container == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(container);
        }

        [HttpPost, ActionName("Container_Delete")]
        public IActionResult Container_DeleteConfirmed(int id)
        {
            var container = _context.Containers.FirstOrDefault(c => c.ContainerId == id);
            if (container == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Containers.Remove(container);
                _context.SaveChanges();
                return RedirectToAction("Container_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(container);
            }
        }

        public IActionResult Container_Details(int id)
        {
            return View(_context.Containers.FirstOrDefault(c => c.ContainerId == id));
        }
    }
}
