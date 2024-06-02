﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
	public class Order_APIController : Controller
	{
		private readonly ILogger<Order_APIController> _logger;
		public OrderRepository ordRepo = new OrderRepository();
		public Order_APIController(ILogger<Order_APIController> logger)
		{
			_logger = logger;
		}
		public STUDY4Context _context = new STUDY4Context();
		public IActionResult Index()
		{
			return View();
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
			existingUser.PhoneNumber = request.PhoneNumber;
			var order = new Order
			{
				UserId = existingUser.UserId,
				CourseId = existingCourse.CourseId,
				TotalAmount = existingCourse.CoursePrice,
				OrderDate = DateTime.Now,
				PhoneNumber = request.PhoneNumber,
				Address = request.Address,
				State = false
			};
			_context.Orders.Add(order);
			await _context.SaveChangesAsync();
			var newlyAddedOrderId = order.OrderId; // Lấy giá trị ID vừa được thêm vào
			return Json(new { status = 200, orderId = newlyAddedOrderId, message = "Course purchased successfully." });
		}


		[HttpPost("Buy_Success")]
		public async Task<IActionResult> Buy_Success(Buy_SuccessRequest order)
		{
			var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == order.OrderId);
			try
			{
				if (!existingOrder.State == true)
				{
					existingOrder.State = true;
					var queryNewUserCourses = new UserCourse
					{
						UserId = existingOrder.UserId,
						CourseId = (int)existingOrder.CourseId,
						Date = DateTime.Now,
					};
					await _context.UserCourses.AddAsync(queryNewUserCourses);
				}
				else
				{
					return BadRequest("You Had Bought Before");
				}
				await _context.SaveChangesAsync();
				return Json(new { status = 200, order = existingOrder, message = "Update Order State Successful" });
			}
			catch (Exception e)
			{
				return BadRequest("Has error when Update State of Order");
			}

		}
	}
}
