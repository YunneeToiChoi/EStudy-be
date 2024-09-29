using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class OfUserIdRequest
    {
        [Required]
        public string userId { get; set; }
    }
}
