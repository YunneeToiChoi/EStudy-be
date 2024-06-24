﻿using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Validation;
using NuGet.Protocol.Core.Types;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using study4_be.Services.Request;
using study4_be.Services;
using study4_be.Controllers.Admin;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;
using FirebaseAdmin.Auth;
using SendGrid.Helpers.Mail;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth_APIController : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private SMTPServices _smtpServices;
        private STUDY4Context _context = new STUDY4Context();
        private UserRegistrationValidator _userRegistrationValidator = new UserRegistrationValidator();
        private readonly ILogger<CoursesController> _logger;
        private FireBaseServices _fireBaseServices;
        public Auth_APIController(ILogger<CoursesController> logger, FireBaseServices fireBaseServices, SMTPServices smtpServices)
        {
            _logger = logger;
            _fireBaseServices = fireBaseServices;
            _smtpServices = smtpServices;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register()
        {
            using (var reader = new StreamReader(HttpContext.Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync();
                var user = JsonSerializer.Deserialize<User>(requestBody);

                if (user != null)
                {
                    string errorMessage;
                    if (_userRegistrationValidator.Validate(user, out errorMessage))
                    {
                        _userRepository.AddUser(user);
                        var link = $"http://localhost:8000/api/Auth_API/{user.UserEmail}/verification={false}";
                        var subject = "[EStudy] - Thông  tin đơn hàng và mã kích hoạt khóa học";
                        var emailContent = _smtpServices.GenerateLinkVerifiByEmailContent(user.UserEmail, link);
                        await _smtpServices.SendEmailAsync(user.UserEmail, subject,emailContent,emailContent);
                        //UserRecord userRecord;
                        //userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                        //{
                        //    Email = user.UserEmail,
                        //    EmailVerified = false,
                        //    Password = user.UserPassword
                        //});
                        // can resend it
                        return Json(new { status = 200, message = "User registered successfully", userData = user });
                    }
                    else
                    {
                        return BadRequest(errorMessage);
                    }
                }
                else
                {
                    return BadRequest("Invalid user data");
                }
            }
        }
        [HttpPost("Login")]
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
                            return Json(new { status = 200, message = "Login successful", user });
                        }
                        return Unauthorized("User is not verification");
                        // can resend it
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
        [HttpGet("Get_AllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> Get_AllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Json(new { status = 200, message = "Get Users Successful", users });

        }
        [HttpPost("Get_UserProfile")]
        public async Task<ActionResult<IEnumerable<User>>> Get_UserProfile([FromBody] OfUserIdRequest _req)
        {
            try
            {
                var user = await _context.Users.Where(u => u.UserId == _req.userId).FirstOrDefaultAsync();
                var response = new
                {
                    userId = user.UserId,
                    userName = user.UserName,
                    userEmail = user.UserEmail,
                    userDescription = user.UserDescription,
                    PhoneNumber = user.PhoneNumber,
                    UserBanner = user.UserBanner,
                    UserImage = user.UserImage,
                };
                return Json(new { status = 200, message = "Get User Profile Successful", user = response });

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //development enviroment
        [HttpDelete("Delete_AllUsers")]
        public async Task<IActionResult> Delete_AllUsers()
        {
            await _userRepository.DeleteAllUsersAsync();
            return Json(new { status = 200, message = "Delete Users Successful" });
        }
        [HttpPost("User_UpdateAvatar")]
        public async Task<IActionResult> User_UpdateAvatar([FromForm] UserUploadImageRequest _req, [FromForm] IFormFile userImage)
        {
            if (_req.userId == null)
            {
                return BadRequest(new { status = 400, message = "User id is not valid" });
            }

            var userExist = _context.Users.FirstOrDefault(u => u.UserId == _req.userId);

            if (userExist == null)
            {
                return NotFound(new { status = 404, message = "User not found" });
            }

            if (userImage == null || userImage.Length == 0)
            {
                return BadRequest(new { status = 400, message = "Invalid image file" });
            }

            var firebaseBucketName = _fireBaseServices.GetFirebaseBucketName();

            // Delete the old avatar image from Firebase Storage if it exists
            if (!string.IsNullOrEmpty(userExist.UserImage))
            {
                // Extract the file name from the URL
                var oldFileName = Path.GetFileName(new Uri(userExist.UserImage).LocalPath);
                await _fireBaseServices.DeleteFileFromFirebaseStorageAsync(oldFileName, firebaseBucketName);
            }

            // Upload the new avatar image to Firebase Storage
            var uniqueId = Guid.NewGuid().ToString();
            var imgFilePath = $"IMG{uniqueId}.jpg";
            string firebaseUrl = await _fireBaseServices.UploadFileToFirebaseStorageAsync(userImage, imgFilePath, firebaseBucketName);

            // Update the user's avatar URL in the database
            userExist.UserImage = firebaseUrl;

            try
            {
                _context.SaveChanges();
                return Json(new { status = 200, message = "User avatar updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating avatar for user with ID {_req.userId}: {ex.Message}");
                return StatusCode(500, new { status = 500, message = "An error occurred while updating the avatar" });
            }
        }
        [HttpPost("ActiveCode")] // active course // nam o user course
        public async Task<IActionResult> ActiveCode([FromBody] ActiveCodeRequest _req)
        {
            var existingOrder = await _context.Orders
                                              .Where(o => o.UserId == _req.userId && o.Code == _req.code)
                                              .FirstOrDefaultAsync();

            if (existingOrder == null)
            {
                return BadRequest(new { status = 400, message = "Activation code is not valid or that code not for this user" });
            }
            try
            {
                if (existingOrder.State == true)
                {
                    var existingUserCourse = await _context.UserCourses
                                                           .Where(uc => uc.UserId == existingOrder.UserId && uc.CourseId == existingOrder.CourseId)
                                                           .FirstOrDefaultAsync();

                    if (existingUserCourse != null) // thieu && state == true
                    {
                        return BadRequest(new { status = 400, message = "You have already activated this course" });
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

                    return Ok(new { status = 200, order = existingOrder, message = "Update User Course Successful" });
                }
                else
                {
                    return BadRequest(new { status = 400, message = "Order is not active" });
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = 500, message = "An error occurred while updating the state of the order", error = e.Message });
            }
        }
        // GET api/verification/{userId}/{verificationCode}
        [HttpGet("{userEmail}/verification={false}")]
        public IActionResult Verify(string userEmail)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserEmail == userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }
            user.Isverified = true;
            _context.SaveChanges();

            return Ok("Verification successful");
        }
        [HttpPost("ResendLink")]
        public async Task<IActionResult> ResendLink([FromBody] ResendLinkActive _req)
        {
           
            // Thực hiện kiểm tra xác thực userId và verificationCode
            // Ví dụ đơn giản: Kiểm tra trong cơ sở dữ liệu
            var user = _context.Users.FirstOrDefault(u => u.UserEmail == _req.userEmail);

            if (user == null)
            {
                return NotFound("User not found");
            }
            if (user.Isverified == true)
            {
                return BadRequest("User had actived");

            }
            var link = $"https://elearning.engineer/api/Auth_API/{user.UserEmail}/verification={false}";
            var subject = "[EStudy] - Thông  tin đơn hàng và mã kích hoạt khóa học";
            var emailContent = _smtpServices.GenerateLinkVerifiByEmailContent(user.UserEmail, link);
            await _smtpServices.SendEmailAsync(user.UserEmail, subject, emailContent, emailContent);

            return Ok("Resend link verification successful");
        }
    }
}
