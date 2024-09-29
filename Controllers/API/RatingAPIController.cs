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
    public class RatingAPI : Controller
    {
        private readonly UserRepository _userRepository = new UserRepository();
        private Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;
        public RatingAPI(FireBaseServices fireBaseServices, Study4Context context)
        {
            _fireBaseServices = fireBaseServices;
            _context = context; 
        }
        [HttpPost("submitRating")]
        public async Task<IActionResult> PostRating([FromForm] RatingSubmitRequest _req, [FromForm] List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate image count (1-5 images only)
            if (images.Count > 5 || images.Count < 1)
            {
                return BadRequest(new { message = "You must upload between 1 and 5 images." });
            }

            try
            {
                // Use a transaction for consistency
                using (var transaction = await _context.Database.BeginTransactionAsync()) // neu 1 trong nhung thao tac that bai -> out 
                {
                    // Validate user existence
                    var user = await _context.Users.FindAsync(_req.userId);
                    if (user == null)
                    {
                        return NotFound(new { message = "User not found" });
                    }

                    // Validate the entity type and existence
                    object entity = null;
                    if (_req.ratingEntityType == "Document")
                    {
                        entity = await _context.Documents.FindAsync(_req.ratingEntityId);
                    }
                    else if (_req.ratingEntityType == "Course")
                    {
                        entity = await _context.Courses.FindAsync(_req.ratingEntityId);
                    }

                    if (entity == null)
                    {
                        return NotFound(new { message = $"{_req.ratingEntityType} not found" });
                    }

                    // Create new Rating object
                    var rating = new Models.Rating()
                    {
                        UserId = _req.userId,
                        EntityType = _req.ratingEntityType,
                        EntityId = _req.ratingEntityId,
                        RatingValue = _req.ratingValue,
                        Review = _req.ratingReview,
                        RatingDate = DateTime.Now
                    };

                    await _context.Ratings.AddAsync(rating);

                    // Upload images asynchronously in parallel
                    var uploadTasks = images.Select(async image =>
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string imageUrl = await _fireBaseServices.UploadImageRatingToFirebaseStorageAsync(image, _req.userId, fileName);

                        return new RatingImage()
                        {
                            RatingId = rating.Id,
                            ImageUrl = imageUrl
                        };
                    });

                    var uploadedImages = await Task.WhenAll(uploadTasks);

                    // Save RatingImage entities to the database
                    await _context.RatingImages.AddRangeAsync(uploadedImages);
                    await _context.SaveChangesAsync();

                    // Commit transaction after successful image uploads and DB inserts
                    await transaction.CommitAsync();

                    return Ok(new { message = "Rating and images submitted successfully!" });
                }
            }
            catch (Exception ex)
            {
                // Detailed logging (consider using logging frameworks like Serilog)
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }

    }
}