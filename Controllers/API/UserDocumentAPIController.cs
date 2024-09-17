using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Validation;
using System.Collections.Immutable;

namespace study4_be.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDocumentAPIController : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private SMTPServices _smtpServices;
        private readonly IConfiguration _configuration;
        private Study4Context _context = new Study4Context();
        private UserRegistrationValidator _userRegistrationValidator = new UserRegistrationValidator();
        private readonly ILogger<UserDocumentAPIController> _logger;
        private readonly FireBaseServices _fireBaseServices;
        private readonly JwtTokenGenerator _jwtServices;

        private readonly IHttpClientFactory _httpClientFactory;
        public UserDocumentAPIController(IConfiguration configuration, ILogger<UserDocumentAPIController> logger, FireBaseServices fireBaseServices, SMTPServices smtpServices, IHttpClientFactory httpClientFactory, JwtTokenGenerator jwtServices)
        {
            _configuration = configuration;
            _logger = logger;
            _fireBaseServices = fireBaseServices;
            _smtpServices = smtpServices;
            _httpClientFactory = httpClientFactory;
            _jwtServices = jwtServices;
        }
        //Course public , Other , course da mua // state  -- done
        // thieu get all course of user , multifile -- done 

        [HttpPost("GetCourseOfUser")]
        public async Task<IActionResult> GetCourseOfUser(string userId)
        {
            try
            {
                var user = await _context.Users
                                          .Where(u => u.UserId == userId)
                                          .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Lấy danh sách khóa học miễn phí
                    var courseFree = await _context.Courses
                                                          .Where(c => c.CoursePrice == 0)
                                                          .ToListAsync();

                    // Lấy danh sách khóa học đã thanh toán của người dùng
                    var courseOfUser_Paied = await _context.UserCourses
                                                           .Where(uc => uc.UserId == userId)
                                                           .Select(uc => uc.Course)
                                                           .ToListAsync();

                    // Tạo đối tượng để trả về kết quả
                    var courseResponse = new
                    {
                        FreeCourses = courseFree,
                        PaidCourses = courseOfUser_Paied
                        // missing other 
                    };

                    return Ok(new
                    {
                        status = 200,
                        course = courseResponse
                    });
                }
                else
                {
                    return BadRequest("User id is not valid");
                }
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }
        [HttpGet("GetCategory")] // maybe in category api
        public async Task<IActionResult> GetCategory()
        {
            try
            {
                var category = await _context.Categories.ToListAsync();
                return Ok(new
                {
                    status = 200,
                    category = category
                });
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadDocuments(IEnumerable<IFormFile> files, string userId, string description, int categoryId, int courseId, bool state)
        {
            if (files == null || !files.Any())
            {
                return BadRequest("No files uploaded.");
            }

            try
            {
                var userExist = await _context.Users.AnyAsync(u => u.UserId == userId);
                if (!userExist)
                {
                    return BadRequest("User does not exist.");
                }

                var uploadedFiles = new List<string>();

                foreach (var file in files)
                {
                    if (file == null || file.Length == 0)
                    {
                        continue; // Skip empty files
                    }

                    var fileName = file.FileName;

                    // Convert IFormFile to Stream
                    using (var fileStream = file.OpenReadStream())
                    {
                        var fileUrl = await _fireBaseServices.UploadFileDocAsync(fileStream, fileName, userId);

                        var userDoc = new Document
                        {
                            UserId = userId,
                            CategoryId = categoryId,
                            CourseId = courseId,
                            Description = description,
                            DownloadCount = 0,
                            FileType = file.ContentType,
                            FileUrl = fileUrl,
                            IsPublic = state,
                            Title = fileName,
                            UploadDate = DateTime.UtcNow,
                        };

                        _context.Documents.Add(userDoc); // Add document to context
                        uploadedFiles.Add(fileUrl);
                    }
                }

                await _context.SaveChangesAsync(); // Save all documents to database

                return Ok(new { status = 200, FileUrls = uploadedFiles });
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }

    }
}
