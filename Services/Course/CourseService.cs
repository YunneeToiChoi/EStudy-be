
using Google;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.Rating;
using study4_be.Services.User;

namespace study4_be.Services.Rating
{
    public class CourseService : ICourseService
    {
        private readonly Study4Context _context;
        public CourseService(Study4Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
        {
            var courses = await _context.Courses.ToListAsync();
            if (courses == null || !courses.Any())
            {
                throw new Exception("No courses found.");
            }
            return courses.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
            }).ToList();
        }

        public async Task<IEnumerable<CourseResponse>> GetUnregisteredCoursesAsync(GetUserCoursesRequest request)
        {
            if (string.IsNullOrEmpty(request.userId))
            {
                return await GetAllCoursesAsync();
            }

            List<int> registeredCourseIds = await GetCoursesUserHasPurchasedAsync(request.userId);
            var unregisteredCourses = await _context.Courses
                .Where(c => !registeredCourseIds.Contains(c.CourseId))
                .ToListAsync();

            if (unregisteredCourses == null || !unregisteredCourses.Any())
            {
                throw new Exception("No unregistered courses found.");
            }

            return unregisteredCourses.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
            }).ToList();
        }

        private async Task<IEnumerable<CourseResponse>> GetOutstandingCoursesForGuestAsync(int amountOutstanding)
        {
            if (amountOutstanding <= 0)
            {
                throw new ArgumentException("AmountOutstanding must be greater than zero.");
            }

            var outstandingCoursesForGuest = await _context.UserCourses
                .GroupBy(uc => uc.CourseId)
                .Take(amountOutstanding)
                .Select(g => g.Key)
                .ToListAsync();

            if (outstandingCoursesForGuest == null || !outstandingCoursesForGuest.Any())
            {
                throw new Exception("No outstanding courses found for guest.");
            }

            var detailedOutstandingCoursesForGuest = await _context.Courses
                .Where(c => outstandingCoursesForGuest.Contains(c.CourseId))
                .ToListAsync();

            return detailedOutstandingCoursesForGuest.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
            }).ToList();
        }

        public async Task<IEnumerable<CourseResponse>> GetOutstandingCoursesUserNotBoughtAsync(GetUserCoursesRequest request)
        {
            if (string.IsNullOrEmpty(request.userId))
            {
                // User is null, show 4 outstanding courses
                return await GetOutstandingCoursesForGuestAsync(4);
            }

            // Fetch the list of courses the user has purchased
            List<int> userPurchasedCourses = await GetCoursesUserHasPurchasedAsync(request.userId);

            // Find outstanding courses that the user has not bought
            var outstandingCourses = await _context.UserCourses
                .Where(uc => !userPurchasedCourses.Contains(uc.CourseId))
                .GroupBy(uc => uc.CourseId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(request.amountOutstanding)
                .ToListAsync();

            if (outstandingCourses == null || !outstandingCourses.Any())
            {
                throw new Exception("No outstanding courses found.");
            }

            // Get the details of those outstanding courses
            var detailedOutstandingCourses = await _context.Courses
                .Where(c => outstandingCourses.Contains(c.CourseId))
                .ToListAsync();

            return detailedOutstandingCourses.Select(course => new CourseResponse
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                CourseDescription = course.CourseDescription,
                CourseImage = course.CourseImage,
                CoursePrice = course.CoursePrice,
                CourseSale = course.CourseSale,
                LastPrice = course.CoursePrice - (course.CoursePrice * course.CourseSale / 100),
            }).ToList();
        }

        public async Task<List<int>> GetCoursesUserHasPurchasedAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.");
            }

            var userPurchasedCourses = await _context.UserCourses
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CourseId)
                .ToListAsync();

            return userPurchasedCourses;
        }

        public async Task DeleteAllCoursesAsync()
        {
            var courses = await _context.Courses.ToListAsync();
            if (courses == null || !courses.Any())
            {
                throw new Exception("No courses available to delete.");
            }
            _context.Courses.RemoveRange(courses);
            await _context.SaveChangesAsync();
        }

    }
}
