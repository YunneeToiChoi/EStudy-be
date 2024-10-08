namespace study4_be.Services.Request.User
{
    public class UserEditRequest
    {
        public string? UserId { get; set; } = null!;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public string? UserDescription { get; set; }
        public string? UserImage { get; set; }
        public string? UserBanner { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
