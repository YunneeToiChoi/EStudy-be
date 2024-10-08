namespace study4_be.Services.User
{
    public class EditPasswordRequest
    {
        public string userId { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public string confirmPassword { get; set; }
    }
}
