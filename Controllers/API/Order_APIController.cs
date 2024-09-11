using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request;
using System.Security.Cryptography;
using System.Text;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
	[ApiController]
	public class Order_APIController : Controller
	{
		private readonly ILogger<Order_APIController> _logger;
		private OrderRepository ordRepo = new OrderRepository();
		private Order_APIController(ILogger<Order_APIController> logger)
		{
			_logger = logger;
		}
		private STUDY4Context _context = new STUDY4Context();
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
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
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
			if (request == null || request.UserId == null || request.CourseId == null)
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
				State = false
			};
			_context.Orders.Add(order);
			await _context.SaveChangesAsync();
			var newlyAddedOrderId = order.OrderId; // Lấy giá trị ID vừa được thêm vào
			return Json(new { status = 200, orderId = newlyAddedOrderId, message = "Course purchased successfully." });
		}
        [HttpPost("Get_AllOrders")]
        public async Task<IActionResult> Get_AllOrders()
        {
			var orderList = await ordRepo.GetAllOrdersAsync();
            return Json(new { status = 200, orderList, message = "Get All Orders successfully." });
        }
    }
}
