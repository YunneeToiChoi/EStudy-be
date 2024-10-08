namespace study4_be.Services.User
{
    public class EditUserProfileRequest
    {
        public string userId { get; set; }
        public string userName { get; set; }
        public string userEmail { get; set; }
        public string userDescription { get; set; }
        public string phoneNumber { get; set; }
    }
}
