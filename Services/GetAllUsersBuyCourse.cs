using System.ComponentModel.DataAnnotations;

namespace study4_be.Services
{
    public class GetAllUsersBuyCourse
    {
        [Required]
        public required int courseId { get; set; }
    }
}
