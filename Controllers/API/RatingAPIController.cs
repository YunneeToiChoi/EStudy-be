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
namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Rating : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        public Rating(FireBaseServices fireBaseServices, Study4Context context)
        {
            _fireBaseServices = fireBaseServices;
            _context = context; 
        }
        [HttpPost("submit")]
        public async Task<IActionResult> PostRating([FromBody] RatingSubmitRequest _req)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                // Kiểm tra nếu User tồn tại
                var user = await _context.Users.FindAsync(_req.UserId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Kiểm tra loại thực thể là Document hay Course
                if (_req.EntityType != "Document" && _req.EntityType != "Course")
                {
                    return BadRequest(new { message = "Invalid entity type. Must be 'Document' or 'Course'." });
                }

                // Kiểm tra xem thực thể có tồn tại không (Document hoặc Course)
                if (_req.EntityType == "Document")
                {
                    var document = await _context.Documents.FindAsync(_req.EntityId);
                    if (document == null)
                    {
                        return NotFound(new { message = "Document not found" });
                    }
                }
                else if (_req.EntityType == "Course")
                {
                    var course = await _context.Courses.FindAsync(_req.EntityId);
                    if (course == null)
                    {
                        return NotFound(new { message = "Course not found" });
                    }
                }

                // Tạo đối tượng Rating mới
                var rating = new Models.Rating()
                {
                    UserId = _req.UserId,
                    EntityType = _req.EntityType, // type of course or document
                    EntityId = _req.EntityId, // id course or document
                    RatingValue = _req.RatingValue, //  1* -> 5*
                    Review = _req.Review, // user comment 
                    RatingDate = DateTime.Now 
                };

                // Thêm Rating vào cơ sở dữ liệu
                await _context.Ratings.AddAsync(rating);
                await _context.SaveChangesAsync();

                // Trả về phản hồi thành công
                return Ok(new { message = "Rating submitted successfully!" });
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
            
        }

    }
}