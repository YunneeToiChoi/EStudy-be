using Microsoft.AspNetCore.Mvc;
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
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Auth_APIController : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();

        private STUDY4Context _context = new STUDY4Context();
        private UserRegistrationValidator _userRegistrationValidator = new UserRegistrationValidator();
        private readonly ILogger<CoursesController> _logger;
        private FireBaseServices _fireBaseServices;
        public Auth_APIController(ILogger<CoursesController> logger, FireBaseServices fireBaseServices)
        {
            _logger = logger;
            _fireBaseServices = fireBaseServices;
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
                        return Json(new { status = 200, message = "Login successful", user });
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
        //development enviroment
        [HttpDelete("Delete_AllUsers")]
        public async Task<IActionResult> Delete_AllUsers()
        {
            await _userRepository.DeleteAllUsersAsync();
            return Json(new { status = 200, message = "Delete Users Successful" });
        }
        [HttpPost("User_UpdateAvatar")]
        public async Task<IActionResult> User_UpdateAvatar([FromForm] UserUploadImageRequest _req, [FromForm]  IFormFile userImage)
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
        [HttpPost("ActiveCode")]
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
                        // thieu state == true
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
    }
}
