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

        public void CheckAndDeleteExpiredOrders()
        {
            var expirationTime = DateTime.Now.AddMinutes(-15);

            // Lấy ra các đơn hàng quá 15 phút và vẫn đang ở trạng thái chưa thanh toán
            var expiredOrders = _study4Context.Orders
                                .Where(o => o.State == false && o.CreatedAt < expirationTime)
                                .ToList();

            if (expiredOrders.Any())
            {
                // Xóa các đơn hàng quá hạn
                _study4Context.Orders.RemoveRange(expiredOrders);
                _study4Context.SaveChanges();
            }
        }

    }
}
