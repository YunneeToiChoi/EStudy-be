using Microsoft.AspNetCore.Antiforgery;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.User
{
    public class ResendLinkActive
    {
        [Required]
        public string userEmail { get; set; }
    }
}
