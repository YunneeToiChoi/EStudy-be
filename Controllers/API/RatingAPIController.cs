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
        public RatingAPI(FireBaseServices fireBaseServices, Study4Context context, IRatingService ratingService)
        {
            _fireBaseServices = fireBaseServices;
            _context = context;
            _ratingService = ratingService;
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
                    .Where(ri=>ri.ReferenceType=="RATING")
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
                var rating = await _context.Ratings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == _req.ratingId);

                if (rating == null)
                {
                    return NotFound(new { message = "Rating not found" });
                }

                // Xác định số trang và số phản hồi mỗi trang (mặc định là 5)
                int pageNumber = _req.pageNumber > 0 ? _req.pageNumber : 1;
                int pageSize = _req.pageSize > 0 ? _req.pageSize : 5;

                List<RatingReply> replies;

                if (_req.parentId < 0 || _req.parentId == 0 ) // case reply of rating
                {
                    // Lấy phản hồi chính cho rating với phân trang
                    replies = await _context.RatingReplies
                        .AsNoTracking()
                        .Where(rp => rp.RatingId == _req.ratingId)
                        .Include(rp => rp.RatingImages)
                        .Include(rp => rp.User)
                        .Skip((pageNumber - 1) * pageSize) // Bỏ qua các phản hồi của trang trước
                        .Take(pageSize) // Lấy số lượng phản hồi theo yêu cầu
                        .ToListAsync();
                }
                else
                {
                    // Lấy phản hồi con với phân trang dựa trên parentId
                    replies = await _context.RatingReplies
                        .AsNoTracking()
                        .Where(rp => rp.RatingId == _req.ratingId && rp.ParentReplyId == _req.parentId)
                        .Include(rp => rp.RatingImages)
                        .Include(rp => rp.User)
                        .Skip((pageNumber - 1) * pageSize) // Bỏ qua các phản hồi của trang trước
                        .Take(pageSize) // Lấy số lượng phản hồi theo yêu cầu
                        .ToListAsync();
                }

                // Sử dụng DTO để trả về
                var response = new ShowReplyResponse
                {
                    RatingId = rating.Id,
                    RatingContent = rating.Review,
                    Replies = replies.Select(rp => new ReplyDto
                    {
                        ReplyId = rp.ReplyId,
                        RatingId = rp.RatingId,
                        ReplyContent = rp.ReplyContent,
                        ReplyDate = rp.ReplyDate,
                        ParentReplyId = rp.ParentReplyId,
                        User = new UserDto
                        {
                            UserId = rp.User.UserId,
                            UserName = rp.User.UserName,
                            UserImage = rp.User.UserImage
                        },
                        Images = rp.RatingImages.Select(img => new ImageDto
                        {
                            ImageUrl = img.ImageUrl
                        }).ToList()
                    }).ToList()
                };

                return Ok(response);
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