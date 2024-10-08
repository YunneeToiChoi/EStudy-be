using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Services.User;
using System.Linq;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSubs_APIController : ControllerBase
    {
        private readonly DateTimeService _datetimeService;
        private readonly Study4Context _context;
        private readonly SubscriptionRepository _subscriptionRepository;

        public UserSubs_APIController(Study4Context context)
        {
            _context = context;
            _subscriptionRepository = new(context);
            _datetimeService = new(context);
        }

        [HttpPost("Check_Expire")]
        public async Task<IActionResult> CheckExpireSubscription(UserRequest request)
        {
            var result = await _datetimeService.GetPlanTimeRemaining(request.userId);
            if (result == null)
            {
                return NotFound(new { messagse = "Your plan is not exist" });
            }

            if (result.Days <= 5 && result.Days > 0)
            {
                return Ok(new { message = "Your subscription is about to end", days = result.Days, hours = result.Hours, minutes = result.Minutes });
            }
            return Ok(new { days = result.Days, hours = result.Hours, minutes = result.Minutes});
        }
        [HttpPost("Get_PlanFromUser")]
        public async Task<ActionResult<IEnumerable<Subscriptionplan>>> Get_PlanFromUser(UserRequest request)
        {
            // Bắt các plan user đã đăng ký
            var userSubscriptions = await _subscriptionRepository.Get_PlanFromUser(request.userId);

            if (userSubscriptions == null || !userSubscriptions.Any())
            {
                return NotFound(new { message = "User does not have any plans" });
            }

            // Tạo list các planId user đã đăng ký
            var planIds = userSubscriptions.Select(us => us.PlanId).ToList();

            // Bắt các plan tồn tại
            var subscriptionPlans = await _context.Subscriptionplans
                .Where(sp => planIds.Contains(sp.PlanId))
                .Select(sp => new
                {
                    sp.PlanId,
                    sp.PlanName
                })
                .ToListAsync();

            // Kết hợp với state
            var result = subscriptionPlans.Select(sp => new
            {
                sp.PlanId,
                sp.PlanName,
                userSubscriptions.FirstOrDefault(us => us.PlanId == sp.PlanId)?.State // Now this runs in memory
            });

            return Ok(new { message = "Retrieved all plans from user", plans = result });
        }
        [HttpDelete("Cancel_Plan")]
        public async Task<IActionResult> Cancel_Plan(UserRequest request)
        {
            var userSubscription = await _context.UserSubs
                .FirstOrDefaultAsync(u => u.UserId == request.userId && u.State == true);
            _context.UserSubs.Remove(userSubscription);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Your plan is cancel"});
        }
    }
}
