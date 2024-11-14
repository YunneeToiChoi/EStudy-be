using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace study4_be.Services.User
{
    public class UserUploadImageRequest
    {
        public string? userId { get; set; }
        public IFormFile? userAvatar { get; set; }
        public IFormFile? userBanner { get; set; }
    }
}
