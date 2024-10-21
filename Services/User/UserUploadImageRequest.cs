using Microsoft.AspNetCore.Mvc;

namespace study4_be.Services.User
{
    public class UserUploadImageRequest
    {
        public string? userId { get; set; }
        public IFormFile userAvatar { get; set; }
        public IFormFile userBanner { get; set; }
    }
}
