using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.Extensions.Logging;
using study4_be.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace study4_be.Services
{
    public class UserCourseExpirationService
    {
        private readonly Study4Context _context;
        private readonly ILogger<UserCourseExpirationService> _logger;

        public UserCourseExpirationService(Study4Context context, ILogger<UserCourseExpirationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ScheduleUserCourseDeletion(string userId)
        {
            try
            {
                // Lấy thời gian hiện tại
                DateTime currentTime = DateTime.Now;

                // Lập lịch công việc để xóa các khóa học của người dùng sau một năm
                var jobId = BackgroundJob.Schedule(() => DeleteUserCourses(userId), currentTime.AddSeconds(5) - currentTime);
                _logger.LogInformation($"Đã lập lịch công việc xóa các khóa học của người dùng với ID {userId} sau một năm. Job ID: {jobId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lập lịch công việc xóa các khóa học của người dùng: {ex.Message}");
            }
        }

        public async Task DeleteUserCourses(string userId)
        {
            try
            {
                // Tìm tất cả các khóa học của người dùng với ID tương ứng
                var userCourses = await _context.UserCourses.Where(uc => uc.UserId == userId).ToListAsync();

                // Xóa các khóa học
                _context.UserCourses.RemoveRange(userCourses);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Đã xóa {userCourses.Count} khóa học của người dùng với ID {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xóa các khóa học của người dùng với ID {userId}: {ex.Message}");
            }
        }
    }
}
