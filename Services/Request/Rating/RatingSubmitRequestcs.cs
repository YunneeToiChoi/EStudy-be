using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Services.Request.Rating
{
    public class RatingSubmitRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string EntityType { get; set; } // "Document" hoặc "Course"
        [Required]
        public int EntityId { get; set; }
        [Required]
        public short RatingValue { get; set; } // Giá trị từ 1-5
        [Required]
        public string Review { get; set; } // Nhận xét (tùy chọn)
    }
}
