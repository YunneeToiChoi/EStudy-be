using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class BuyCourseRequest
    {
        public string? UserId { get; set; }
        [Required]
        public int CourseId { get; set; }
        public DateTime? OrderDate { get; set; }
        public double? TotalAmount { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
    }
}
