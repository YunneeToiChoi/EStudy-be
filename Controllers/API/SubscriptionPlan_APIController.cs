using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request;
using study4_be.Services.Response;
using System.Security.Cryptography;
using System.Text;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionPlan_APIController : ControllerBase
    {
        private readonly ILogger<SubscriptionPlan_APIController> _logger;
        private readonly SubscriptionRepository _subscriptionRepository;
        private readonly Study4Context _context;

        public SubscriptionPlan_APIController(Study4Context context, ILogger<SubscriptionPlan_APIController> logger) 
        {   _context = context; 
            _subscriptionRepository = new(context);
            _logger = logger;
        }

        private string GenerateOrderId(string userId, int planId)
        {
            using (var sha256 = SHA256.Create())
            {
                var baseString = $"{userId}-{planId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                return ToBase32String(hashBytes).Substring(0, 32); // Increase length to 32 characters
            }
        }
        [HttpGet]
        private string ToBase32String(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            StringBuilder result = new StringBuilder((bytes.Length + 4) / 5 * 8);
            int bitIndex = 0;
            int currentByte = 0;

            while (bitIndex < bytes.Length * 8)
            {
                if (bitIndex % 8 == 0)
                {
                    currentByte = bytes[bitIndex / 8];
                }

                int dualByte = currentByte << 8;
                if ((bitIndex / 8) + 1 < bytes.Length)
                {
                    dualByte |= bytes[(bitIndex / 8) + 1];
                }

                int index = (dualByte >> (16 - (bitIndex % 8 + 5))) & 31;
                result.Append(alphabet[index]);

                bitIndex += 5;
            }

            return result.ToString();
        }

        [HttpPost("Order_Plan")]
        public async Task<IActionResult> Order_Plan([FromBody] OrderPlanRequest request)
        {
            if (request == null || request.UserId == null)
            {
                return BadRequest("Invalid user or plan information.");
            }

            var existingUser = await _context.Users.FindAsync(request.UserId);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }
            var existingOrder = await _context.UserSubs
               .FirstOrDefaultAsync(o => o.UserId == request.UserId && o.PlanId == request.PlanId);

            if (existingOrder != null)
            {
                return BadRequest("You have already ordered this plan.");
            }

            var existingPlan = await _context.Subscriptionplans.FindAsync(request.PlanId);
            if (existingPlan == null)
            {
                return NotFound("Plan not found.");
            }
            var orderId = GenerateOrderId(request.UserId, request.PlanId);
            var order = new UserSub
            {
               UsersubsId = orderId,
               UserId = existingUser.UserId,
               PlanId = existingPlan.PlanId,
               UsersubsTotal = existingPlan.PlanPrice,
               UsersubsStartdate = DateTime.Now,
               UsersubsEnddate = DateTime.Now.AddDays(existingPlan.PlanDuration),
               State = false
            };
            _context.UserSubs.Add(order);
            await _context.SaveChangesAsync();
            var newlyAddedOrderId = order.UsersubsId; // Lấy giá trị ID vừa được thêm vào
            return Ok(new { status = 200, orderId = newlyAddedOrderId, message = "Plan purchased successfully" });
            //return JsonResult(new { status = 200, orderId = newlyAddedOrderId, message = "Course purchased successfully." });
        }
        [HttpGet("Get_AllPlans")]
        public async Task<ActionResult<IEnumerable<Subscriptionplan>>> Get_AllPlans()
        {
            try
            {
                var plans = await _subscriptionRepository.GetAllPlans();
                if(plans == null)
                {
                    return NotFound(new {status = 404, message = "No subscription plans found." });
                }
                var plansResponse = plans.Select(plans => new PlanResponse
                {
                    PlanId = plans.PlanId,
                    PlanName = plans.PlanName,
                    PlanDescription = plans.PlanDescription,
                    PlanDuration = plans.PlanDuration,
                    PlanPrice = plans.PlanPrice,
                });
                return Ok(new { status = 200, message = "Success", data = plansResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving subscription plans.");
                return StatusCode(500, new { message = "An error occurred while retrieving subscription plans." });
            }
        }

        [HttpPost("Get_OutstadingPlans")]
        public async Task<ActionResult> Get_OutstandingPlansUserNotBought(GetUserPlansRequest req)
        {
            if (req.userId == null || req.userId == "")
            {
                var outstandingPlansForGuest = await _context.UserSubs
                .GroupBy(uc => uc.PlanId)
                .Take(req.amountOutstanding)
                .Select(g => g.Key)// Chọn ra PlanId
                .ToListAsync();

                var detailedOutstandingPlansForGuest = await _context.Subscriptionplans
                      .Where(c => outstandingPlansForGuest.Contains(c.PlanId)) // Lọc các gói theo danh sách các PlanId nổi bật
                      .ToListAsync();
                var detailedOutstandingPlansForGuestResponse = detailedOutstandingPlansForGuest.Select(plan => new PlanResponse
                {
                   PlanId = plan.PlanId,
                   PlanName = plan.PlanName,
                   PlanPrice = plan.PlanPrice,
                   PlanDuration = plan.PlanDuration,

                }).ToList();
                return Ok(new { status = 200, message = "Get Outstanding Plans For Guest Successful", outstandingPlans = detailedOutstandingPlansForGuestResponse });
            }

            List<int> userPurchasedPlans = await GetPlansUserHasPurchasedAsync(req.userId);
            var outstandingPlans = await _context.UserSubs
                .Where(uc => !userPurchasedPlans.Contains(uc.PlanId)) // Lọc các khóa học chưa mua
                .GroupBy(uc => uc.PlanId) // Nhóm theo CourseId
                .OrderByDescending(g => g.Count()) // Sắp xếp giảm dần theo số lần xuất hiện
                .Select(g => g.Key)// Chọn ra CourseId
                .Take(req.amountOutstanding)
                .ToListAsync();

            // Lấy thông tin chi tiết của các khóa học nổi bật
            var detailedOutstandingPlans = await _context.Subscriptionplans
                .Where(c => outstandingPlans.Contains(c.PlanId)) // Lọc các khóa học theo danh sách các CourseId nổi bật
                .ToListAsync();

            if (detailedOutstandingPlans == null || !detailedOutstandingPlans.Any())
            {
                return NotFound(new { status = 404, message = "No outstanding plans found or not have any outstaind plans" });
            }
            var detailedOutstandingPlansResponse = detailedOutstandingPlans.Select(plan => new PlanResponse
            {
                PlanId = plan.PlanId,
                PlanName = plan.PlanName,
                PlanPrice = plan.PlanPrice,
                PlanDuration = plan.PlanDuration,
            }).ToList();
            return Ok(new { status = 200, message = "Get Outstanding Plans User Hadn't Bought Successful", outstandingCourses = detailedOutstandingPlansResponse });
        }

        [HttpPost]
        public async Task<List<int>> GetPlansUserHasPurchasedAsync(string userId)
        {
            var userPurchaseBought = await _context.UserSubs
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.PlanId)
                .ToListAsync();
            return userPurchaseBought;
        }
    }
}
