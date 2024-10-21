using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Services.Exam;

namespace study4_be.Services
{
    public class DateTimeService
    {
        private readonly Study4Context _study4Context;

        public DateTimeService(Study4Context study4Context) { _study4Context = study4Context; }
        public async Task<TimeRemainingRespone?> GetPlanTimeRemaining(string userId)
        {
            var userSubscription = await _study4Context.UserSubs
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (userSubscription != null)
            {
                var timeRemaining = userSubscription.UsersubsEnddate - DateTime.UtcNow;
                // Chuyển đổi thời gian còn lại thành ngày, giờ, phút
                var days = timeRemaining.Days;
                var hours = timeRemaining.Hours;
                var minutes = timeRemaining.Minutes;
                return new TimeRemainingRespone(days, hours, minutes);
      
            }
            return null;
        }

        public async Task CheckAndDeleteExpiredOrders()
        {
            var expirationTime = DateTime.Now.AddMinutes(-1);

            // Lấy ra các đơn hàng quá 15 phút và vẫn đang ở trạng thái chưa thanh toán
            var expiredOrders = await _study4Context.Orders
                                .Where(o => o.State == false && o.CreatedAt < expirationTime)
                                .ToListAsync();

            if (expiredOrders.Any())
            {
                // Xóa các đơn hàng quá hạn
                _study4Context.Orders.RemoveRange(expiredOrders);
                await _study4Context.SaveChangesAsync();
            }
        }
        public async Task CheckAndExpireSubscriptions()
        {
            var currentDate = DateTime.Now;

            // Retrieve all active subscriptions that have expired
            var expiringSubscriptions = await _study4Context.UserSubs
                                        .Where(us => us.UsersubsEnddate < currentDate && us.State == true)
                                        .ToListAsync();

            if (expiringSubscriptions.Any())
            {
                foreach (var userSubscription in expiringSubscriptions)
                {
                    // Retrieve the courses associated with the plan from the PLAN_COURSES table
                    var planCourses = await _study4Context.PlanCourses
                                                    .Where(pc => pc.PlanId == userSubscription.PlanId)
                                                    .Select(pc => pc.CourseId)
                                                    .ToListAsync();

                    // Retrieve the courses that the user has explicitly purchased (saved in the ORDERS table)
                    var purchasedCourses = await _study4Context.Orders
                                                         .Where(o => o.UserId == userSubscription.UserId && o.CourseId != null)
                                                         .Select(o => o.CourseId)
                                                         .ToListAsync();

                    // Disable the courses associated with the plan that were not explicitly purchased by the user
                    var userCoursesToDisable = await _study4Context.UserCourses
                                                             .Where(uc => uc.UserId == userSubscription.UserId && planCourses.Contains(uc.CourseId) && !purchasedCourses.Contains(uc.CourseId))
                                                             .ToListAsync();

                    foreach (var userCourse in userCoursesToDisable)
                    {
                        userCourse.State = false; // Mark the course as inactive
                    }

                    // Soft delete the subscription by setting its state to false
                    userSubscription.State = false;
                }

                // Save all changes to the database
                await _study4Context.SaveChangesAsync();
            }
        }
        public async Task CheckAndExpireUserCourse()
        {
            // Calculate the date one year ago from today
            var oneYearAgo = DateTime.UtcNow.AddYears(-1);

            // Query for all user_course records where the course has been active for more than a year
            var expiredCourses = await _study4Context.UserCourses
                                    .Where(uc => uc.Date < oneYearAgo && uc.State == true)
                                    .ToListAsync();

            // If any courses are found, update their state to false
            if (expiredCourses.Any())
            {
                foreach (var course in expiredCourses)
                {
                    course.State = false;
                }
                // Save the changes to the database
                await _study4Context.SaveChangesAsync();
            }
        }
        public static DateTime ConvertToVietnamTime(DateTime utcDateTime)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, vietnamTimeZone);
            return vietnamTime;
        }
    }
}
