using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Models.ViewModel;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.User;

namespace study4_be.Interface
{
    public interface IUserService
    {
        Task RegisterUserAsync(Models.User user);
        Task<bool> LogoutAsync();
        Task<IEnumerable<Models.User>> GetAllUsersAsync();
        Task<UserProfileResponse> GetUserProfileAsync(string userId);
        Task<bool> EditUserProfileAsync(EditUserProfileRequest request);
        Task<bool> EditPasswordAsync(EditPasswordRequest request);
        Task<bool> DeleteAllUsersAsync();
        Task<(bool success, string message)> UpdateUserImageAsync(UserUploadImageRequest request, IFormFile userAvatar, IFormFile userBanner);
        Task<(bool success, string message)> ActivateCodeAsync(ActiveCodeRequest request);
        Task<(bool success, string message)> RequestForgotPasswordAsync(OfUserEmailRequest request);
        Task<(bool success, string message)> VerifyUserEmailAsync(string userEmail);
        Task<(bool success, string message)> GetDataResetPasswordAsync(string userEmail, string currentTime);
        Task<(bool success, string message)> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<(bool success, string message)> ResendLinkAsync(ResendLinkActive _req);
    }
}
