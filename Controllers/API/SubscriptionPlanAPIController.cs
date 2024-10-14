using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Plan;
using study4_be.Services.User;
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


       
        [HttpPost("Get_AllPlans")]
        public async Task<ActionResult<IEnumerable<Subscriptionplan>>> Get_AllPlans(UserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.userId)) 
                {
                    var plans = await _subscriptionRepository.GetAllPlans();
                    if (plans == null)
                    {
                        return NotFound(new { status = 404, message = "No subscription plans found." });
                    }
                    var plansResponse = plans.Select(plans => new PlanResponse
                    {
                        PlanId = plans.PlanId,
                        PlanName = plans.PlanName,
                        PlanDescription = plans.PlanDescription,
                        PlanDuration = plans.PlanDuration,
                        PlanPrice = plans.PlanPrice,
                    });
                    return Ok(new { status = 200, message = "Success", plans = plansResponse });
                }
                else
                {
                    // Bắt các plan user đã đăng ký
                    var userSubscriptions = await _subscriptionRepository.Get_PlanFromUser(request.userId);

                    // Tạo list các planId user đã đăng ký
                    var planIds = userSubscriptions.Select(us => us.PlanId).ToList();

                    // Bắt tất cả các plan tồn tại trong database
                    var subscriptionPlans = await _context.Subscriptionplans
                        .Select(sp => new
                        {
                            sp.PlanId,
                            sp.PlanName,
                            sp.PlanDescription,
                            sp.PlanDuration,
                            sp.PlanPrice
                        })
                        .ToListAsync();

                    // Kết hợp với state
                    var result = subscriptionPlans.Select(sp => new
                    {
                        sp.PlanId,
                        sp.PlanName,
                        sp.PlanDescription,
                        sp.PlanDuration,
                        sp.PlanPrice,
                        userSubscriptions.FirstOrDefault(us => us.PlanId == sp.PlanId)?.State // Trả về state nếu user đã mua plan, ngược lại null
                    });

                    return Ok(new { message = "Retrieved all plans", plans = result });
                }

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
