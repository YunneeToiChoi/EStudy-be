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
using iText.IO.Image;
using study4_be.Models.DTO;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface;
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingAPI : ControllerBase
    {
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        private readonly IRatingService _ratingService;
        private readonly IReplyService _replyService;
        public RatingAPI(FireBaseServices fireBaseServices, Study4Context context, IRatingService ratingService, IReplyService replyService)
        {
            _fireBaseServices = fireBaseServices;
            _context = context;
            _ratingService = ratingService;
            _replyService = replyService;
        }
        [HttpGet("GetAllRating")]
        public async Task<IActionResult> GetAllRating()
        {

            var ratings = await _context.Ratings
               .Select(r => new
               {
                   ratingId = r.Id,
                   userId = r.UserId,
                   ratingEntityType = r.EntityType,
                   ratingValue = r.RatingValue,
                   ratingReview = r.Review,
                   ratingDate = r.RatingDate,
                   ratingImageUrls = r.RatingImages.Select(ri => ri.ImageUrl).ToList(), // Chỉ lấy hình ảnh từ RatingImages
                   replyExist = _context.RatingReplies.Any(rr => rr.RatingId == r.Id) // Kiểm tra xem có phản hồi không
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
           .Where(r => r.EntityType == "Document" && r.DocumentId == _req.documentId)
               .Select(r => new RatingDocumentResponse
               {
                   ratingId = r.Id,
                   userId = r.UserId,
                   userImage = r.User.UserImage,
                   documentId = r.DocumentId,
                   ratingValue = r.RatingValue,
                   ratingReview = r.Review,
                   ratingDate = r.RatingDate,
                   ratingImageUrls = r.RatingImages
                .Where(ri => ri.ReferenceType == "RATING")
                .Select(ri => ri.ImageUrl)
                .ToList(),
                   replyExist = _context.RatingReplies.Any(rp => rp.RatingId == r.Id) // Kiểm tra sự tồn tại của phản hồi
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
                .Where(r => r.EntityType == "Course" && r.CourseId == _req.courseId)
                .Select(r => new RatingCourseResponse
                {
                    ratingId = r.Id,
                    userId = r.UserId,
                    userImage = r.User.UserImage,
                    courseId = r.CourseId,
                    ratingValue = r.RatingValue,
                    ratingReview = r.Review,
                    ratingRatingDate = r.RatingDate,
                    ratingImageUrls = r.RatingImages
                    .Where(ri => ri.ReferenceType == "RATING")
                    .Select(ri => ri.ImageUrl)
                    .ToList(),
                    replyExist = _context.RatingReplies.Any(rp => rp.RatingId == r.Id) // Kiểm tra sự tồn tại của phản hồi
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
        [HttpPost("SubmitRatingOrReply")]
        public async Task<IActionResult> SubmitRatingOrReply([FromForm] RatingOrReplySubmitRequest request, [FromForm] List<IFormFile> ratingImages)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ.", errors = ModelState });
            }

            if (ratingImages.Count > 5)
            {
                return BadRequest(new { message = "Chỉ cho phép đăng từ 0 - 5 ảnh" });
            }

            try
            {
                var result = await _ratingService.SubmitRatingOrReplyAsync(request, ratingImages);
                return Ok(new { message = "Dữ liệu đã được gửi thành công!", data = result });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi không xác định.", details = ex.Message });
            }
        }
        [HttpPost("ShowReply")]
        public async Task<IActionResult> ShowReply(ShowReplyRequest _req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data is not valid", errors = ModelState });
            }
            try
            {
                var response = await _replyService.ShowReplyAsync(_req);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { message = "Database error, please try again later.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
    }
}