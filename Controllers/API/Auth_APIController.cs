using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Validation;
using NuGet.Protocol.Core.Types;
using System.IO;
using System.Text.Json;
using study4_be.Services;
using Microsoft.EntityFrameworkCore;
using study4_be.Models.ViewModel;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using study4_be.Services.User;
using study4_be.Interface;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth_APIController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly SMTPServices _smtpServices;
        private readonly IConfiguration _configuration;
        private readonly Study4Context _context;
        private readonly UserRegistrationValidator _userRegistrationValidator;
        private readonly ILogger<Auth_APIController> _logger;
        private readonly FireBaseServices _fireBaseServices;
        private readonly JwtTokenGenerator _jwtServices;
        private readonly IUserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;
        public Auth_APIController(IConfiguration configuration ,
            ILogger<Auth_APIController> logger, 
            FireBaseServices fireBaseServices, 
            SMTPServices smtpServices, 
            IHttpClientFactory httpClientFactory, 
            JwtTokenGenerator jwtServices, 
            Study4Context context,
            IUserService userService)
        {
            _configuration = configuration;
            _logger = logger;
            _fireBaseServices = fireBaseServices;
            _smtpServices = smtpServices;
            _httpClientFactory = httpClientFactory;
            _jwtServices = jwtServices;
            _context = context;
            _userService = userService; 
            _userRepository = new(context,configuration);
            _userRegistrationValidator = new(_userRepository);
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register()
        {
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody);

                if (user == null)
                {
                    return BadRequest("Invalid user data");
                }

                try
                {
                    await _userService.RegisterUserAsync(user);
                    return Ok(new { status = 200, message = "User registered successfully", userData = user });
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpPost("Login")] // missing interface in there 
        public async Task<IActionResult> Login()
        {
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync();
                var loginData = JsonSerializer.Deserialize<User>(requestBody);

                if (loginData != null && !string.IsNullOrEmpty(loginData.UserEmail) && !string.IsNullOrEmpty(loginData.UserPassword))
                {
                    var user = _userRepository.GetUserByUserEmail(loginData.UserEmail);

                    if (user != null && _userRepository.VerifyPassword(loginData.UserPassword, user.UserPassword))
                    {
                        if (user.Isverified == true)
                        {
                            var token = _jwtServices.GenerateToken(user.UserName, user.UserEmail, user.UserId, 1);
                            return Json(new { status = 200, message = "Login successful", user ,token});
                        }
                        return Unauthorized("User is not verification");
                    }
                    else
                    {
                        return Unauthorized("Invalid username or password");
                    }
                }
                else
                {
                    return BadRequest("Invalid login data");
                }
            }
        }
        private async Task<User> GetOrCreateUser(string userId, string userName, string userEmail, string userAvatar)
        {
            // Kiểm tra người dùng có tồn tại không
            var userExist = await _context.Users
                .Where(u => u.UserId == userId || u.UserEmail == userEmail).FirstOrDefaultAsync();
            var emailExist = _context.Users.Where(u => u.UserEmail == userEmail).FirstOrDefaultAsync();
            var idExist = _context.Users.Where(u => u.UserId== userId).FirstOrDefaultAsync();
            var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();
            var imgDefault = _configuration["Firebase:AvatarDefaultUser"];//default img
            if (userExist == null)
            {
                // maybe use repository add new user in there 
                // Nếu người dùng chưa tồn tại, tạo mới
                var newUser = new User
                {
                    UserId = Guid.NewGuid().ToString(),// still gen id 
                    UserName = userName,
                    UserEmail = userEmail,
                    UserImage = imgDefault, 
                    Isverified = true // Assuming new users are verified
                };
                // Update the user's avatar URL in the database
                if (!(userAvatar == null || userAvatar.Length == 0) && !string.IsNullOrEmpty(newUser.UserImage))
                {
                    // Upload the new avatar image to Firebase Storage
                    var uniqueId = Guid.NewGuid().ToString();
                    var imgFilePath = $"IMG{uniqueId}.jpg";
                    string firebaseUrl = await _fireBaseServices.UploadFileFromUrlToFirebaseStorageAsync(userAvatar,
                                                                                                    imgFilePath,
                                                                                                    firebaseBucketName);
                    // Extract the file name from the URL
                    //var oldFileName = Path.GetFileName(new Uri(newUser.UserImage).LocalPath);
                    //await _fireBaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName); // don't delete
                    newUser.UserImage = firebaseUrl;
                    
                }
                // Thêm người dùng vào cơ sở dữ liệu và lưu thay đổi
                await _userRepository.AddUserWithServices(newUser);
                await _context.SaveChangesAsync();
                return newUser;
            }
            else if (idExist == null && emailExist != null)
            {
                var user = await _context.Users.Where(u => u.UserEmail == userEmail).FirstOrDefaultAsync();
                return user;
            }
            return userExist; // Trả về người dùng nếu đã tồn tại
        } // missing interface in there 

        //############ GOOGLE ############// 

        [HttpGet("signin-google")]
        public IActionResult SignInGoogle()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", "Auth_API")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]// missing interface in there 
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return RedirectToAction("Error", "Home");
            }

            var accessToken = result.Properties.GetTokenValue("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("No Access Token found.");
            }
            try
            {
                string frontEndUrl = _configuration["Url:FrontEnd"];
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={accessToken}");
                var userInfo = JObject.Parse(response);

                var userId = userInfo["id"].ToString();
                var userName = userInfo["name"].ToString();
                var userEmail = userInfo["email"].ToString();
                var userAvatar = userInfo["picture"]?.ToString();

                var user = await GetOrCreateUser(userId, userName, userEmail, userAvatar);
                var token = _jwtServices.GenerateToken(user.UserName, user.UserEmail, user.UserId, 1);
                var htmlContent = $@"
                    <script>
                        // Gửi thông điệp chứa JWT token về cửa sổ cha (React frontend)
                        window.opener.postMessage({{ token: '{token}' }}, '{frontEndUrl}');
                        // Đóng cửa sổ popup
                        window.close();
                    </script>";
                return Content(htmlContent, "text/html");

            }
            catch (Exception ex)
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }


        //############ FACEBOOK ############// 
        [HttpPost("facebook-login")] // missing interface in there 
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.accessToken))
            {
                return BadRequest(new { status = 400, message = "Access token is required" });
            }

            try
            {
                var accessToken = model.accessToken;
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync($"https://graph.facebook.com/me?access_token={accessToken}&fields=id,name,email,picture");
                var userInfo = JObject.Parse(response);

                var userId = userInfo["id"]?.ToString();
                var userName = userInfo["name"]?.ToString();
                var userEmail = userInfo["email"]?.ToString() ?? "No email available";
                var userAvatar = userInfo["picture"]?["data"]?["url"]?.ToString() ?? "No avatar available";

                // Sử dụng hàm chung để lấy hoặc tạo người dùng
                var user = await GetOrCreateUser(userId, userName, userEmail, userAvatar);

                // Tạo JWT token
                var token = _jwtServices.GenerateToken(user.UserName, user.UserEmail, user.UserId, 1);


                return Ok(new
                {
                    status = 200,
                    token,
                    user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { status = 500, message = ex.Message });
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _userService.LogoutAsync();
            if (result)
            {
                // Invalidate the user session, clear cookies, etc., if needed
                return Ok(new { message = "Logged out successfully" });
            }

            return BadRequest(new { message = "Logout failed" });
        }

        [HttpGet("Get_AllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> Get_AllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(new { status = 200, message = "Get Users Successful", users });
        }
        [HttpPost("Get_UserProfile")]
        public async Task<ActionResult<UserProfileResponse>> Get_UserProfile([FromBody] OfUserIdRequest _req)
        {
            try
            {
                var userProfile = await _userService.GetUserProfileAsync(_req.userId);
                if (userProfile == null)
                {
                    return NotFound(new { status = 404, message = "User not found" });
                }

                return Ok(new { status = 200, message = "Get User Profile Successful", user = userProfile });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("EditUserProfile")]
        public async Task<IActionResult> EditUserProfile([FromBody] EditUserProfileRequest request)
        {
            try
            {
                var result = await _userService.EditUserProfileAsync(request);
                if (!result)
                {
                    return NotFound(new { status = 404, message = "User not found" });
                }

                return Ok(new { status = 200, message = "User profile updated successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = $"An error occurred while updating the user profile: {e.Message}" });
            }
        }
        [HttpPost("EditPassword")]
        public async Task<IActionResult> EditPassword([FromBody] EditPasswordRequest request)
        {
            try
            {
                var result = await _userService.EditPasswordAsync(request);
                if (!result)
                {
                    return BadRequest(new { status = 400, message = "Old password is incorrect or user not found" });
                }

                return Ok(new { status = 200, message = "Password updated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { status = 400, message = ex.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = $"An error occurred while updating the password: {e.Message}" });
            }
        }

        [HttpDelete("Delete_AllUsers")]
        public async Task<IActionResult> Delete_AllUsers()
        {
            await _userService.DeleteAllUsersAsync();
            return Json(new { status = 200, message = "Delete Users Successful" });
        }
        [HttpPost("User_UpdateImage")]
        public async Task<IActionResult> User_UpdateImage([FromForm] UserUploadImageRequest request, [FromForm] IFormFile userAvatar, [FromForm] IFormFile userBanner)
        {
            var (success, message) = await _userService.UpdateUserImageAsync(request, userAvatar, userBanner);
            if (!success)
            {
                return BadRequest(new { status = 400, message });
            }

            return Ok(new { status = 200, message });
        }

        [HttpPost("ActivateCode")]
        public async Task<IActionResult> ActivateCode(ActiveCodeRequest request)
        {
            var (success, message) = await _userService.ActivateCodeAsync(request);
            if (!success)
            {
                return BadRequest(new { status = 400, message });
            }

            return Ok(new { status = 200, message });
        }
        [HttpPost("RequestForgotPassword")]
        public async Task<IActionResult> RequestForgotPassword(OfUserEmailRequest _req)
        {
            var result = await _userService.RequestForgotPasswordAsync(_req);
            if (!result.success)
            {
                return BadRequest(new { status = 400, message = result.message });
            }
            return Ok(new { status = 200, message = result.message });
        }
        [HttpGet("userEmail={userEmail}/verification={false}")]
        public async Task<IActionResult> Verify(string userEmail)
        {
            var result = await _userService.VerifyUserEmailAsync(userEmail);
            if (!result.success)
            {
                return NotFound(result.message);
            }

            return View("Verification");
        }
        [HttpGet("userEmail={userEmail}/verification={false}/time={currentTime}")]
        public async Task<IActionResult> GetDataResetPassword(string userEmail, string currentTime)
        {
            var result = await _userService.GetDataResetPasswordAsync(userEmail, currentTime);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            var model = new ResetPasswordViewModel { userEmail = userEmail };
            return View("ResetPassword", model);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            var result = await _userService.ResetPasswordAsync(model);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(result.message);
        }

        [HttpPost("ResendLink")]
        public async Task<IActionResult> ResendLink([FromBody] ResendLinkActive _req)
        {
            var result = await _userService.ResendLinkAsync(_req);
            if (!result.success)
            {
                return BadRequest(result.message);
            }

            return Ok(result.message);
        }
    }
}
