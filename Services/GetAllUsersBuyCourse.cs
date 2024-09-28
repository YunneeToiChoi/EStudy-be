using System.ComponentModel.DataAnnotations;

namespace study4_be.Services
{
    public class GetAllUsersBuyCourse
    {
        [Required]
        public int courseId { get; set; }
    }
}
