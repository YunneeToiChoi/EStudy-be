using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{

    [Route("Admin/CourseManager/[controller]/[action]")]
    public class SubscriptionplanController : Controller
    {
        private readonly ILogger<SubscriptionplanController> _logger;
        private readonly Study4Context _context;
        private readonly SubscriptionRepository _subscriptionRepository;
        public SubscriptionplanController(ILogger<SubscriptionplanController> logger, Study4Context context)
        {
            _logger = logger;
            _context = context;
            _subscriptionRepository = new(context);
        }

        public async Task<ActionResult<IEnumerable<Subscriptionplan>>> GetAllPlans()
        {
            var plans = await _subscriptionRepository.GetAllPlans();
            return Json(new { status = 200, message = "Get Plans Successful", plans });

        }
        //development enviroment
        public async Task<IActionResult> DeleteAllCourses()
        {
            await _subscriptionRepository.DeleteAllPlansAsync();
            return Json(new { status = 200, message = "Delete Plans Successful" });
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Subscriptionplan_List()
        {
            var plans = await _subscriptionRepository.GetAllPlans(); // Retrieve list of courses from repository
            return View(plans); // Pass the list of courses to the view
        }
        public IActionResult Subscriptionplan_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Subscriptionplan_Create(Subscriptionplan plan)
        {
            if (!ModelState.IsValid)
            {

                return View(plan);    //show form with value input and show errors
            }
            try
            {
                await _context.AddAsync(plan);
                await _context.SaveChangesAsync();
                CreatedAtAction(nameof(GetSubscriptionplanById), new { id = plan.PlanId }, plan);
                return RedirectToAction("Subscriptionplan_List", "Subscriptionplan"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new plan.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(plan);
            }
        }

        public async Task<IActionResult> GetSubscriptionplanById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var plan = await _context.Subscriptionplans.FindAsync(id);
            if (plan == null)
            {
                return NotFound(new { message = "Plan is not found" });
            }

            return Ok(plan);
        }
        [HttpGet]
        public async Task<IActionResult> Subscriptionplan_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "planId is invalid" });
            }
            var plan = await _context.Subscriptionplans.FirstOrDefaultAsync(c => c.PlanId == id);
            if (plan == null)
            {
                return NotFound(new { message = "Plan is not found" });
            }
            return View(plan);
        }

        [HttpPost]
        public async Task<IActionResult> Subscriptionplan_Edit(Subscriptionplan plan)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Plan not found");
                return NotFound();
            }
            var planToUpdate = await _context.Subscriptionplans.FirstOrDefaultAsync(c => c.PlanId == plan.PlanId);
            if (planToUpdate == null)
            {
                _logger.LogError($"Plan not found");
                return RedirectToAction("Subscriptionplan_List");
            }
            try
            {
                planToUpdate.PlanName = plan.PlanName;
                planToUpdate.PlanDescription = plan.PlanDescription;
                planToUpdate.PlanPrice = plan.PlanPrice;
                planToUpdate.PlanDuration = plan.PlanDuration;
                await _context.SaveChangesAsync();
                return RedirectToAction("Subscriptionplan_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plan");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the plan.");
            }
            return View(plan);
        }
        [HttpGet]
        public async Task<IActionResult> Subscriptionplan_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Plan not found for deletion.");
                return NotFound($"Plan not found.");
            }
            var plan = await _context.Subscriptionplans.FirstOrDefaultAsync(c => c.PlanId == id);
            if (plan == null)
            {
                _logger.LogError($"Plan not found for delete.");
                return NotFound($"Plan not found.");
            }
            return View(plan);
        }

        [HttpPost, ActionName("Subscriptionplan_Delete")]
        public async Task<IActionResult> Subscriptionplan_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Plan not found for deletion.");
                return NotFound($"Plan not found.");
            }
            var plan = await _context.Subscriptionplans.FirstOrDefaultAsync(c => c.PlanId == id);
            if (plan == null)
            {
                _logger.LogError($"Plan not found for deletion.");
                return NotFound($"Plan not found.");
            }
            try
            {
                _context.Subscriptionplans.Remove(plan);
                await _context.SaveChangesAsync();
                return RedirectToAction("Subscriptionplan_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plan");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the plan.");
                return View(plan);
            }
        }


        public async Task<IActionResult> Subscriptionplan_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid plan ID.");
                TempData["ErrorMessage"] = "The specified plan was not found.";
                return RedirectToAction("Subscriptionplan_List", "Subscriptionplan");
            }

            var plan = await _context.Subscriptionplans.FirstOrDefaultAsync(c => c.PlanId == id);

            // If no container is found, return to the list with an error
            if (plan == null)
            {
                TempData["ErrorMessage"] = "The specified plan was not found.";
                return RedirectToAction("Subscriptionplan_List", "Subscriptionplan");
            }
            return View(plan);
        }
    }
}
