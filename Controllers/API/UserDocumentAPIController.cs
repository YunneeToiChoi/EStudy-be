using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;
using study4_be.Validation;
using System.Collections.Immutable;
using PdfSharpCore.Pdf;
using System.Drawing.Imaging;
using PdfiumViewer;
using Microsoft.CodeAnalysis;
using study4_be.Models.DTO;
using study4_be.Services.User;
using study4_be.Interface;
using study4_be.Interface.Rating;
using study4_be.Services.Rating;
using System.Security.Claims;
using study4_be.Services.Course;
using study4_be.Services.Document;
using System.Reflection.Metadata.Ecma335;

namespace study4_be.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDocumentAPIController : ControllerBase
    {
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly ICourseService _courseService;

        public UserDocumentAPIController(
            FireBaseServices fireBaseServices, 
            Study4Context context,
            IDocumentService documentService,
            ICategoryService categoryService,
            ICourseService courseService)
        {
            _fireBaseServices = fireBaseServices;
            _context = context;
            _documentService = documentService;
            _categoryService = categoryService;
            _courseService = courseService;
        }
        //################################################### DOCUMENT ##########################################//
        [HttpPost("GetDocByCourse")]
        public async Task<IActionResult> GetDocByCourse(OfCourseIdRequest _req)
        {
            if (_req.courseId <= 0)
            {
                return BadRequest($"Course id is invalid: {_req.courseId}");
            }

            var documents = await _documentService.GetDocumentsByCourseAsync(_req);

            if (documents == null)
            {
                // Check if course exists and return appropriate response
                var courseExist = await _context.Courses.AnyAsync(c => c.CourseId == _req.courseId);
                if (!courseExist)
                {
                    return NotFound($"Course with Id {_req.courseId} does not exist.");
                }

                return StatusCode(500, "An error occurred while retrieving documents.");
            }

            return Ok(new
            {
                status = 200,
                message = "Get All Documents Successful",
                documents
            });
        }

        //################################################### DOCUMENT ##########################################//

        [HttpGet("GetAllCate")]
        public async Task<IActionResult> GetAllCate()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(new
                {
                    status = 200,
                    message = "Get All Categories Successful",
                    categories
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 500, message = ex.Message });
            }
        }
        [HttpGet("GetAllCourse")]
        public async Task<IActionResult> GetAllCourse()
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync();
                return Ok(new
                {
                    status = 200,
                    message = "Get All Courses Successful",
                    courses
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 500, message = ex.Message });
            }
        }
        [HttpGet("GetAllDoc")]
        public async Task<IActionResult> GetAllDoc()
        {
            try
            {
                var document = await _context.Documents
                       .Include(c => c.Category) // Assuming a relationship exists
                       .Include(c => c.Course)   // Assuming a relationship exists
                       .ToListAsync();
                var documentResponse = document.Select(c => new
                {
                    documentId = c.DocumentId,
                    title = c.Title,
                    documentPublic = c.IsPublic,
                    uploadDate = c.UploadDate,
                    price = c.Price,
                    fileType = c.FileType,
                    isPublic = c.IsPublic,
                    downloadCount = c.DownloadCount,
                    categoryId = c.CategoryId,
                    categoryName = c.Category != null ? c.Category.CategoryName : "Unknown",
                    courseId = c.Course != null ? c.Course.CourseId : (int?)null, // Sử dụng kiểu nullable int
                    courseName = c.Course != null ? c.Course.CourseName : "Unknown",
                    documentDescription = c.Description,
                    thumbnailUrl = c.ThumbnailUrl
                }).ToList();
                return Ok(new
                {
                    status = 200,
                    message = "Get All Document Successful",
                    document = documentResponse
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("GetDocByUser")]
        public async Task<IActionResult> GetDocByUser([FromBody] OfUserIdRequest _req)
        {
            if (_req == null || string.IsNullOrEmpty(_req.userId))
            {
                return BadRequest("User ID is required.");
            }

            try
            {
                var userDocResponses = await _documentService.GetDocumentsByUserIdAsync(_req.userId);
                return Ok(new
                {
                    status = 200,
                    userDoc = userDocResponses
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("User ID does not exist.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }


        [HttpPost("DownloadDocument")]
        public async Task<IActionResult> DownloadDocument([FromBody] OfDocumentIdRequest _req)
        {
            var result = await _documentService.DownloadDocumentAsync(_req.documentId, _req.userId);
            return result;
        }
        [HttpPost("GetUserDocumentProfile/{userId}")]
        public async Task<IActionResult> GetUserDocumentProfile(string userId) // missing db user document, interface 
        {
            try
            {
                // Lấy thông tin người dùng từ cơ sở dữ liệu
                var user = await _context.Users
                    .Include(u => u.Documents)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                // Tạo đối tượng UserProfileDto
                var userProfileDto = new UserProfileDto
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserEmail = user.UserEmail,
                    UserDescription = user.UserDescription,
                    UserImage = user.UserImage,
                    UserBanner = user.UserBanner,
                    PhoneNumber = user.PhoneNumber,
                    Documents = user.Documents.Select(doc => new DocumentDto
                    {
                        documentId = doc.DocumentId,
                        title = doc.Title,
                        documentDescription = doc.Description,
                        fileUrl = doc.FileUrl,
                        uploadDate = doc.UploadDate,
                        fileType = doc.FileType,
                        downloadCount = doc.DownloadCount,
                        thumbnailUrl = doc.ThumbnailUrl
                    }).ToList()
                };

                return Ok(userProfileDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }
        //################################################### BASE #############################################//
        //################################################### UPLOAD ##########################################//
        [HttpPost("GetCourseOfUser")]
        public async Task<IActionResult> GetCourseOfUser([FromBody] OfUserIdRequest _req)
        {
            try
            {
                var courseResponse = await _documentService.GetCoursesByUserIdAsync(_req.userId);
                return Ok(new
                {
                    status = 200,
                    course = courseResponse
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest($"Error occurred: {e.Message}");
            }
        }
        [HttpPost("GetDocumentId")]
        public async Task<IActionResult> GetDocumentId(string orderId)  
        {
            try
            {
                var documentId = await _documentService.GetDocumentIdAsync(orderId);
                if (documentId == null)
                {
                    // Nếu documentId là null, trả về lỗi
                    return NotFound("Document not found");
                }
                return Ok(new { documentId });
            }
            catch (Exception ex)
            {
                // Trả về lỗi chi tiết khi gặp exception
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpPost("GetDocumentFromUser")]
        public async Task<IActionResult> GetDocumentFromUser(UserRequest request)
        {
            try
            {
                var userDocument = await _documentService.GetDocumentsFromUserAsync(request.userId);

                return Ok(userDocument);
            }
            catch (Exception ex) 
            {
                return BadRequest($"Error occurred: {ex.Message}");
            }
        }
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadDocuments(UploadDocumentRequest _req)
        {
            return await _documentService.UploadDocuments(_req);
        }

        [HttpPost("UploadDetail")]
        public async Task<IActionResult> UploadDetail(UploadDetailRequest _req)
        {
            return await _documentService.UploadDetail(_req);
        }
        [HttpGet("DocumentDetail/{documentId}")]
        public async Task<IActionResult> GetDocumentDetail(int documentId)
        {
            try
            {
                var documentDetail = await _documentService.GetDocumentDetailAsync(documentId);
                return Ok(documentDetail);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}
