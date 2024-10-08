using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request.User
{
    public class GetAllUsersBuyCourse
    {
        [Required]
        public required int courseId { get; set; }
    }
}
