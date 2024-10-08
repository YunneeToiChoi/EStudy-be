using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Services.Rating
{
    public class RatingResponse
    {
        public int ratingId { get; set; }
        public string userId { get; set; } = null!;
        public string ratingEntityType { get; set; } = null!;
        public int ratingEntityId { get; set; }
        public int ratingValue { get; set; }
        public string? ratingReview { get; set; }
        public DateTime ratingDate { get; set; }
        public List<string> ratingImages { get; set; } = new List<string>();
    }
}
