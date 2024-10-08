using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.User
{
    public class GetAllUnitsByCourses
    {
        [Required]
        public required int courseId { get; set; }
        public string userId { get; set; } = string.Empty;
    }
}
