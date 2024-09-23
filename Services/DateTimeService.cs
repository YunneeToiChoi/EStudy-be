using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Services.Response;

namespace study4_be.Services
{
    public class DateTimeService
    {
        private readonly Study4Context _study4Context = new();

        public async Task<TimeRemainingRespone?> GetPlanTimeRemaining(string userId)
        {
            var userSubscription = await _study4Context.UserSubs
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (userSubscription != null && userSubscription.State)
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
    }
}
