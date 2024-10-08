using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Crmf;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Order;
using study4_be.Services.Payment;
using System.Security.Cryptography;
using System.Text;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
	[ApiController]
	public class Order_APIController : Controller
	{
		private readonly OrderRepository ordRepo;
		private readonly Study4Context _context;
		public Order_APIController(Study4Context context)
		{
			this._context = context;
			ordRepo = new(context);
		}
		[HttpGet]
        private string GenerateOrderId(string userId, int courseId)
        {
            using (var sha256 = SHA256.Create())
            {
                var baseString = $"{userId}-{courseId}-{DateTime.UtcNow.Ticks}-{Guid.NewGuid()}";
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(baseString));
                return ToBase32String(hashBytes).Substring(0, 32); // Increase length to 32 characters
            }
        }
        [HttpGet]
        private string ToBase32String(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXY1Z23456789";
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
        //development enviroment
        [HttpDelete("Delete_AllOrders")]
		public async Task<IActionResult> Delete_AllOrders()
		{
			await ordRepo.DeleteAllOrdersAsync();
			return Json(new { status = 200, message = "Delete All Orders Successful" });
		}
		[HttpPost("Buy_Course")] // thieu number phone va tru thoi gian
		public async Task<IActionResult> Buy_Course([FromBody] BuyCourseRequest request)
		{
            if (request == null)
            {
                return BadRequest("Request cannot be null.");
            }

            if (string.IsNullOrEmpty(request.UserId) || request.CourseId <= 0)
            {
                return BadRequest("Invalid user or course information.");
            }
            var existingUser = await _context.Users.FindAsync(request.UserId);
			if (existingUser == null)
			{
				return NotFound("User not found.");
			}
			var existingOrder = await _context.Orders
			   .FirstOrDefaultAsync(o => o.UserId == request.UserId && o.CourseId == request.CourseId);

			if (existingOrder != null)
			{
				return BadRequest("You have already ordered this course.");
			}

			var existingCourse = await _context.Courses.FindAsync(request.CourseId);
			if (existingCourse == null)
			{
				return NotFound("Course not found.");
			}
            var orderId = GenerateOrderId(request.UserId, request.CourseId);
            var order = new Order
			{
                OrderId = orderId,
                UserId = existingUser.UserId,
				CourseId = existingCourse.CourseId,
				TotalAmount = existingCourse.CoursePrice,
				OrderDate = DateTime.Now,
				Email = request.Email,
				Address = request.Address,
				CreatedAt = DateTime.Now,
				State = false
			};
			_context.Orders.Add(order);
			await _context.SaveChangesAsync();
			var newlyAddedOrderId = order.OrderId; // Lấy giá trị ID vừa được thêm vào
			return Json(new { status = 200, orderId = newlyAddedOrderId, message = "Course purchased successfully." });
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
            var order = new Order
            {
                OrderId = orderId,
                UserId = existingUser.UserId,
                PlanId = existingPlan.PlanId,
                TotalAmount = existingPlan.PlanPrice,
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                State = false
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var newlyAddedOrderId = order.OrderId; // Lấy giá trị ID vừa được thêm vào
            return Ok(new { status = 200, orderId = newlyAddedOrderId, message = "Plan purchased successfully" });
        }
        [HttpPost("Renew_Plan")]
        public async Task<IActionResult> Renew_Plan([FromBody] OrderPlanRequest request)
        {
            var existingUserSub = await _context.UserSubs.FindAsync(request.UserId, request.PlanId);
            if(existingUserSub == null)
            {
                return NotFound(new { message = "User's subscription plan is not found" });
            }

            var existingPlan = await _context.Subscriptionplans.FindAsync(request.PlanId);
            if (existingPlan == null)
            {
                return NotFound("Plan not found.");
            }
            if (existingUserSub.State)
            {
                return BadRequest(new { message = "User's subsciption plan still available" });
            }
            var orderId = GenerateOrderId(existingUserSub.UserId, existingUserSub.PlanId);
            var order = new Order
            {
                OrderId = orderId,
                UserId = existingUserSub.UserId,
                PlanId = existingUserSub.PlanId,
                TotalAmount = existingPlan.PlanPrice,
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                State = false
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var newlyAddedOrderId = order.OrderId; // Lấy giá trị ID vừa được thêm vào
            return Ok(new { status = 200, orderId = newlyAddedOrderId, message = "Renew Plan successfully" });
        }
        [HttpPost("Renew_Course")]
        public async Task<IActionResult> Renew_Course([FromBody] BuyCourseRequest request)
        {
            var existingUserCourse = await _context.UserCourses.FindAsync(request.UserId, request.CourseId);
            if (existingUserCourse == null) 
            {
                return NotFound(new { message = "User's course is not found" });
            }
            var existingCourse = await _context.Courses.FindAsync(request.CourseId);
            if (existingCourse == null)
            {
                return NotFound("Course not found.");
            }
            if (existingUserCourse.State)
            {
                return BadRequest(new { message = "User's course still available" });
            }
            var orderId = GenerateOrderId(existingUserCourse.UserId, existingUserCourse.CourseId);
            var order = new Order
            {
                OrderId = orderId,
                UserId = existingUserCourse.UserId,
                CourseId = existingUserCourse.CourseId,
                TotalAmount = existingCourse.CoursePrice,
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                State = false
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            var newlyAddedOrderId = order.OrderId; // Lấy giá trị ID vừa được thêm vào
            return Json(new { status = 200, orderId = newlyAddedOrderId, message = "Renew Course successfully." });
        }
        [HttpPost("Get_AllOrders")]
        public async Task<IActionResult> Get_AllOrders()
        {
			var orderList = await ordRepo.GetAllOrdersAsync();
            return Json(new { status = 200, orderList, message = "Get All Orders successfully." });
        }
    }
}
