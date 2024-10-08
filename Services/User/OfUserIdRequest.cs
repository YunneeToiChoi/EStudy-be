using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.User
{
    public class OfUserIdRequest
    {
        [Required]
        public string userId { get; set; }
    }
}
