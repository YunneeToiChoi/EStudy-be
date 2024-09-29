using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    [Route("[controller]/{action=Index}")]
    public class ContainerController : Controller
    {
        private readonly ILogger<ContainerController> _logger;
        public ContainerController(ILogger<ContainerController> logger)
        {
            _logger = logger;
        }
        private readonly ContainerRepository _containersRepository = new ContainerRepository();
        public Study4Context _context = new Study4Context();
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
        public async Task<IActionResult> Container_Create()
        {
            var units = await _context.Units.Include(u => u.Course).ToListAsync();
            var model = new ContainerCreateViewModel
            {
                containers = new Container(){},
                listUnits = units.Select(c => new SelectListItem
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
            if (!ModelState.IsValid)
            {
                containerViewModel.listUnits = await _context.Units.Select(c => new SelectListItem
                {
                    Value = c.UnitId.ToString(),
                    Text = c.UnitTittle + " : " + c.Course.CourseName
                }).ToListAsync();
                return View(containerViewModel);
            }
            try
            {
                var container = new Container
                {
                    ContainerId = containerViewModel.containers.ContainerId,
                    ContainerTitle = containerViewModel.containers.ContainerTitle,
                    UnitId = containerViewModel.containers.UnitId,
                };

                await _context.AddAsync(container);
                await _context.SaveChangesAsync();

                return RedirectToAction("Container_List", "Container");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                containerViewModel.listUnits = await _context.Units.Select(c => new SelectListItem
                {
                    Value = c.UnitId.ToString(),
                    Text = c.UnitTittle + " : " + c.Course.CourseName
                }).ToListAsync();
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
        public async Task<IActionResult> Container_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            var container = await _context.Containers.FirstOrDefaultAsync(c => c.ContainerId == id);
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
                var courseToUpdate = await _context.Containers.FirstOrDefaultAsync(c => c.ContainerId == container.ContainerId);
                courseToUpdate.ContainerTitle = container.ContainerTitle;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Container_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(container);
        }

        [HttpGet]
        public async Task<IActionResult> Container_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Container not found for delete.");
                return NotFound($"Container not found.");
            }
            var container = await _context.Containers.FirstOrDefaultAsync(c => c.ContainerId == id);
            if (container == null)
            {
                _logger.LogError($"Container not found for delete.");
                return NotFound($"Container not found.");
            }
            return View(container);
        }

        [HttpPost, ActionName("Container_Delete")]
        public async Task<IActionResult> Container_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid) 
            {
                _logger.LogError($"Container not found for deletion.");
                return NotFound($"Container not found.");
            }
            var container = await _context.Containers.FirstOrDefaultAsync(c => c.ContainerId == id);
            if (container == null)
            {
                _logger.LogError($"Container not found for deletion.");
                return NotFound($"Container not found.");
            }

            try
            {
                _context.Containers.RemoveRange(container);
                await _context.SaveChangesAsync();
                return RedirectToAction("Container_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting container: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the container.");
                return View(container);
            }
        }

        public async Task<IActionResult> Container_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid container ID.");
                TempData["ErrorMessage"] = "The specified container was not found.";
                return RedirectToAction("Container_List", "Container");
            }

            var container = await _context.Containers.FirstOrDefaultAsync(c => c.ContainerId == id);

            // If no container is found, return to the list with an error
            if (container == null)
            {
                TempData["ErrorMessage"] = "The specified container was not found.";
                return RedirectToAction("Container_List", "Container");
            }

            return View(container);
        }
    }
}
