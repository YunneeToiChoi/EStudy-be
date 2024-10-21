
using Google.Apis.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using study4_be.Interface;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services.User;
using study4_be.Validation;

namespace study4_be.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        private readonly UserRegistrationValidator _userRegistrationValidator;
        private readonly SMTPServices _smtpService;
        private readonly IConfiguration _configuration;
        private readonly Study4Context _context;
        private readonly ILogger<UserService> _logger;
        private readonly FireBaseServices _fireBaseServices;
        public UserService(
            UserRepository userRepository,
            UserRegistrationValidator userRegistrationValidator,
            SMTPServices emailService,
            IConfiguration configuration,
            Study4Context context,
            ILogger<UserService> logger,
            FireBaseServices fireBaseServices)
        {
            _userRepository = userRepository;
            _userRegistrationValidator = userRegistrationValidator;
            _smtpService = emailService;
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _fireBaseServices = fireBaseServices;
        }

        public async Task RegisterUserAsync(Models.User user)
        {
            string errorMessage;
            if (_userRegistrationValidator.Validate(user, out errorMessage))
            {
                await _userRepository.AddUserAsync(user);
                string beUrl = _configuration["Url:BackEnd"];
                var link = $"{beUrl}/api/Auth_API/userEmail={user.UserEmail}/verification={false}";
                var subject = "[EStudy] - Yêu cầu xác thực tài khoản của bạn";
                var emailContent = _smtpService.GenerateLinkVerifiByEmailContent(user.UserEmail, link);
                await _smtpService.SendEmailAsync(user.UserEmail, subject, emailContent, emailContent);
            }
            else
            {
                throw new ArgumentException(errorMessage);
            }
        }
        public async Task<bool> LogoutAsync()
        {
            return true; 
        }

        public async Task<IEnumerable<Models.User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }
        public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new UserProfileResponse
                {
                    userId = u.UserId,
                    userName = u.UserName,
                    userEmail = u.UserEmail,
                    userDescription = u.UserDescription,
                    phoneNumber = u.PhoneNumber,
                    userBanner = u.UserBanner,
                    userImage = u.UserImage
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<bool> EditUserProfileAsync(EditUserProfileRequest request)
        {
            var user = await _context.Users
                .Where(u => u.UserId == request.userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return false; // User not found
            }

            if (!string.IsNullOrEmpty(request.userName))
            {
                user.UserName = request.userName;
            }

            if (!string.IsNullOrEmpty(request.userEmail))
            {
                user.UserEmail = request.userEmail;
            }

            if (!string.IsNullOrEmpty(request.userDescription))
            {
                user.UserDescription = request.userDescription;
            }

            if (!string.IsNullOrEmpty(request.phoneNumber))
            {
                user.PhoneNumber = request.phoneNumber;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true; // Update successful
        }
        public async Task<bool> EditPasswordAsync(EditPasswordRequest request)
        {
            var user = await _context.Users
                .Where(u => u.UserId == request.userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return false; // User not found
            }

            if (!_userRepository.VerifyPassword(request.oldPassword, user.UserPassword))
            {
                return false; // Old password is incorrect
            }

            if (request.newPassword != request.confirmPassword)
            {
                throw new InvalidOperationException("New password and confirm password do not match");
            }

            user.UserPassword = request.newPassword;
            _userRepository.HashPassword(user);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true; // Password updated successfully
        }

        public async Task<bool> DeleteAllUsersAsync()
        {
            await _userRepository.DeleteAllUsersAsync();
            return true; // Users deleted successfully
        }
        public async Task<(bool success, string message)> UpdateUserImageAsync(UserUploadImageRequest request)
        {
            var userExist = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.userId);
            if (userExist == null)
            {
                return (false, "User not found.");
            }

            var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();

            // Handle user avatar update
            if (request.userAvatar != null && request.userAvatar.Length > 0)
            {
                try
                {
                    var uniqueId = Guid.NewGuid().ToString();
                    var imgFilePath = $"IMG{uniqueId}.jpg";
                    string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(request.userAvatar, imgFilePath, firebaseBucketName);

                    // Delete old image from Firebase
                    var oldFileName = Path.GetFileName(new Uri(userExist.UserImage).LocalPath);
                    await _fireBaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName);
                    userExist.UserImage = firebaseUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user avatar.");
                    return (false, "Error updating avatar. Please try again later.");
                }
            }

            // Handle user banner update
            if (request.userBanner != null && request.userBanner.Length > 0)
            {
                try
                {
                    var uniqueId = Guid.NewGuid().ToString();
                    var imgFilePath = $"IMG{uniqueId}.jpg";
                    string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(request.userBanner, imgFilePath, firebaseBucketName);

                    // Delete old banner from Firebase
                    var oldFileName = Path.GetFileName(new Uri(userExist.UserBanner).LocalPath);
                    await _fireBaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName);
                    userExist.UserBanner = firebaseUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user banner.");
                    return (false, "Error updating banner. Please try again later.");
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return (true, "Images updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to user images.");
                return (false, "An error occurred while saving changes. Please try again later.");
            }
        }
        public async Task<(bool success, string message)> ActivateCodeAsync(ActiveCodeRequest request)
        {
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.UserId == request.userId && o.Code == request.code);

            if (existingOrder == null)
            {
                _logger.LogWarning("Activation code is not valid or not for this user");
                return (false, "Mã kích hoạt không hợp lệ hoặc không phải của người dùng này.");
            }

            try
            {
                if (existingOrder.State == false)
                {
                    _logger.LogWarning("Order is not active");
                    return (false, "Đơn hàng không hoạt động.");
                }

                var existingUserCourse = await _context.UserCourses
                    .FirstOrDefaultAsync(uc => uc.UserId == existingOrder.UserId && uc.CourseId == existingOrder.CourseId);

                if (existingUserCourse != null)
                {
                    _logger.LogWarning("Course already activated");
                    return (false, "Khóa học đã được kích hoạt.");
                }

                var newUserCourse = new UserCourse
                {
                    UserId = existingOrder.UserId,
                    CourseId = (int)existingOrder.CourseId,
                    Date = DateTime.Now,
                    Process = 0,
                    State = true
                };

                await _context.UserCourses.AddAsync(newUserCourse);
                await _context.SaveChangesAsync();

                return (true, "Kích hoạt khóa học thành công."); // Kích hoạt thành công
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error activating course.");
                return (false, "Đã xảy ra lỗi trong quá trình kích hoạt khóa học."); // Thông báo lỗi cho người dùng
            }
        }
        public async Task<(bool success, string message)> RequestForgotPasswordAsync(OfUserEmailRequest _req)
        {
            if (_req.userEmail == null)
            {
                return (false, "User email cannot be null.");
            }

            try
            {
                var userExist = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == _req.userEmail);
                if (userExist == null)
                {
                    return (false, "User does not exist.");
                }

                var urlPort = _configuration["Url:BackEnd"];
                var currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
                var link = $"{urlPort}/api/Auth_API/userEmail={userExist.UserEmail}/verification={false}/time={currentTime}";
                var subject = "[EStudy] - Yêu cầu đặt lại mật khẩu của bạn";
                var emailContent = _smtpService.GenerateLinkVerifiByEmailContent(userExist.UserEmail, link);

                // Send email
                await _smtpService.SendEmailAsync(userExist.UserEmail, subject, emailContent, emailContent);

                return (true, "Send link to reset password successful.");
            }
            catch (Exception e)
            {
                return (false, $"An error occurred: {e.Message}");
            }
        }
        public async Task<(bool success, string message)> VerifyUserEmailAsync(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);

            if (user == null)
            {
                return (false, "User not found.");
            }

            user.Isverified = true;
            await _context.SaveChangesAsync();

            return (true, "User verification successful.");
        }
        public async Task<(bool success, string message)> GetDataResetPasswordAsync(string userEmail, string currentTime)
        {
            if (!DateTime.TryParseExact(currentTime, "yyyyMMddHHmmss",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime queryTime))
            {
                return (false, "Invalid time format.");
            }

            DateTime currentTimeNow = DateTime.Now;
            TimeSpan timeDifference = currentTimeNow - queryTime;

            if (timeDifference.TotalMinutes > 10)
            {
                return (false, "Reset password link expired.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == userEmail);
            if (user == null)
            {
                return (false, "User not found.");
            }

            user.Isverified = true;
            await _context.SaveChangesAsync();

            return (true, "Password reset link is valid.");
        }
        public async Task<(bool success, string message)> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            if (model.newPassword != model.confirmPassword)
            {
                return (false, "Passwords do not match.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == model.userEmail);
            if (user == null)
            {
                return (false, "User not found.");
            }

            user.UserPassword = model.newPassword;
            _userRepository.HashPassword(user);
            await _context.SaveChangesAsync();

            return (true, "Password has been reset successfully.");
        }
        public async Task<(bool success, string message)> ResendLinkAsync(ResendLinkActive _req)
        {
            if (string.IsNullOrWhiteSpace(_req.userEmail))
            {
                return (false, "User email must be provided.");
            }

            var user = await _context.Users.FindAsync(_req.userEmail);
            if (user == null)
            {
                return (false, "User not found.");
            }

            if (user.Isverified == true)
            {
                return (false, "User has already been activated.");
            }

            string beUrl = _configuration["Url:BackEnd"];
            var link = $"{beUrl}/api/Auth_API/userEmail={user.UserEmail}/verification={false}";
            var subject = "[EStudy] - Thông tin đơn hàng và mã kích hoạt khóa học";
            var emailContent = _smtpService.GenerateLinkVerifiByEmailContent(user.UserEmail, link);
            await _smtpService.SendEmailAsync(user.UserEmail, subject, emailContent, emailContent);

            return (true, "Resend link verification successful.");
        }
    }
}