using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace study4_be.Services.User
{
    public class UserUploadImageRequest
    {
        public string? userId { get; set; }

        [Required(ErrorMessage = "The userAvatar field is required.")]
        public IFormFile userAvatar { get; set; }

        [Required(ErrorMessage = "The userBanner field is required.")]
        public IFormFile userBanner { get; set; }
    }
}
