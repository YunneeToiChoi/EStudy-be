using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Validation;
using NuGet.Protocol.Core.Types;
using System.IO;
using System.Text.Json;
using study4_be.Services.Request;
using study4_be.Services;
using Microsoft.EntityFrameworkCore;
using study4_be.Models.ViewModel;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using study4_be.Services.Request.Rating;
using study4_be.Services.Request.Document;
using Microsoft.CodeAnalysis;
using study4_be.Services.Response;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingAPI : ControllerBase
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        public RatingAPI(FireBaseServices fireBaseServices, Study4Context context)
        {
            _fireBaseServices = fireBaseServices;
            _context = context; 
        }
        [HttpGet("GetAllRating")]
        public async Task<IActionResult> GetRatingsOfDocument()
        {
            var ratings = await _context.Ratings
                .Include(r => r.RatingImages) 
                .Select(r => new
                {
                    ratingId = r.Id,
                    userId = r.UserId,
                    ratingEntityType = r.EntityType,
                    ratingEntityId = r.EntityId,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList() 
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Đánh giá tài liệu được lấy thành công.",
                data = ratings
            });
        }
        [HttpPost("RatingOfDocument")]
        public async Task<IActionResult> GetRatingsOfDocument(OfDocumentIdRequest _req)
        {
                var ratings = await _context.Ratings
               .Include(r => r.RatingImages) // Nạp dữ liệu liên quan
               .Include(r => r.User) // Nạp dữ liệu người dùng
               .Where(r => r.EntityType == "Document" && r.EntityId == _req.documentId)
                   .Select(r => new RatingDocumentResponse
                {
                    ratingId = r.EntityId,
                    userId = r.UserId,
                    userImage = r.User.UserImage,
                    documentId = r.EntityId,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Đánh giá tài liệu được lấy thành công.",
                data = ratings
            });
        }

        // Lấy đánh giá của khóa học
        [HttpPost("RatingOfCourse")]
        public async Task<IActionResult> GetRatingsOfCourse(OfCourseIdRequest _req)
        {
            var ratings = await _context.Ratings
                .Where(r => r.EntityType == "Course" && r.EntityId == _req.courseId)
                .Select(r => new RatingCourseResponse
                {
                    ratingId = r.EntityId,
                    userId = r.UserId,
                    userImage = r.User.UserImage,
                    courseId = r.EntityId,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingRatingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Đánh giá khóa học được lấy thành công.",
                data = ratings
            });
        }

        // Lấy đánh giá của người dùng
        [HttpPost("RatingOfUser")]
        public async Task<IActionResult> GetRatingsOfUser(OfUserIdRequest _req)
        {
            var ratings = await _context.Ratings
                .Where(r => r.UserId == _req.userId)
                .Select(r => new RatingUserResponse
                {
                    ratingId = r.Id,
                    userId = r.UserId,
                    userImage = r.User.UserImage,
                    ratingEntityId = r.EntityId,
                    ratingEntityType = r.EntityType,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Đánh giá của người dùng được lấy thành công.",
                data = ratings
            });
        }
        [HttpPost("SubmitRating")]
        public async Task<IActionResult> PostRating([FromForm] RatingSubmitRequest _req, [FromForm] List<IFormFile> ratingImages)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors = ModelState });
            }
            if (ratingImages.Count > 5 || ratingImages.Count < 0)
            {
                return BadRequest(new { message = "Chỉ cho phép đăng từ 0 - 5 ảnh" });
            }
            try
            {
                // Kiểm tra xem người dùng có tồn tại không
                var user = await _context.Users.FindAsync(_req.userId);
                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại." });
                }

                // Bắt đầu một giao dịch
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    // Tạo một Rating mới
                    var rating = new Rating
                    {
                        UserId = _req.userId,
                        EntityType = _req.ratingEntityType,
                        EntityId = _req.ratingEntityId,
                        RatingValue = _req.ratingValue,
                        Review = _req.ratingReview,
                        RatingDate = DateTime.Now
                    };

                    // Thêm Rating vào cơ sở dữ liệu
                    await _context.Ratings.AddAsync(rating);
                    await _context.SaveChangesAsync(); // Lưu để nhận Id mới

                    // Giờ đây, chúng ta có thể an toàn thêm các RatingImages
                    var imageUrls = new List<string>(); // Danh sách để lưu URL hình ảnh
                    foreach (var image in ratingImages)
                    {
                        // Tải lên hình ảnh và nhận URL
                        string fileName = Path.GetFileName(image.FileName);
                        string imageUrl = await _fireBaseServices.UploadImageRatingToFirebaseStorageAsync(image, _req.userId, fileName);
                        imageUrls.Add(imageUrl); // Thêm URL vào danh sách

                        // Tạo RatingImage và liên kết nó với Rating mới
                        var ratingImage = new RatingImage
                        {
                            RatingId = rating.Id, // Sử dụng Id của Rating mới tạo
                            ImageUrl = imageUrl
                        };

                        await _context.RatingImages.AddAsync(ratingImage);
                    }
                    // Cam kết giao dịch
                    await transaction.CommitAsync();
                    await _context.SaveChangesAsync();

                    // Tạo đối tượng RatingResponse
                    var response = new RatingResponse
                    {
                        ratingId = rating.Id,
                        userId = rating.UserId,
                        ratingEntityType = rating.EntityType,
                        ratingEntityId = rating.EntityId,
                        ratingValue = rating.RatingValue,
                        ratingReview = rating.Review,
                        ratingDate = rating.RatingDate,
                        ratingImages = imageUrls 
                    };

                    return Ok(new { message = "Đánh giá đã được gửi thành công!", rating = response });
                }
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Lỗi cập nhật cơ sở dữ liệu: {ex.InnerException?.Message}");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi gửi đánh giá. Vui lòng thử lại sau." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi không xác định.", details = ex.Message });
            }
        }
    }
}