﻿using Microsoft.AspNetCore.Mvc;
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
using Microsoft.CodeAnalysis;
using iText.IO.Image;
using study4_be.Models.DTO;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.Rating;
using study4_be.Services.User;
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
            var ratings = await _ratingService.GetAllRatingsAsync();
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
            var ratings = await _ratingService.GetRatingsOfDocumentAsync(_req);
            return Ok(new
            {
                success = true,
                message = "Đánh giá tài liệu được lấy thành công.",
                data = ratings
            });
        }

        [HttpPost("RatingOfCourse")]
        public async Task<IActionResult> GetRatingsOfCourse(OfCourseIdRequest _req)
        {
            var ratings = await _ratingService.GetRatingsOfCourseAsync(_req);
            return Ok(new
            {
                success = true,
                message = "Đánh giá khóa học được lấy thành công.",
                data = ratings
            });
        }

        [HttpPost("RatingOfUser")]
        public async Task<IActionResult> GetRatingsOfUser(OfUserIdRequest _req)
        {
            var ratings = await _ratingService.GetRatingsOfUserAsync(_req);
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