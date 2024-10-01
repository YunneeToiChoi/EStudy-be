public class UserProfileDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string UserEmail { get; set; }
    public string? UserDescription { get; set; }
    public string? UserImage { get; set; }
    public string? UserBanner { get; set; }
    public string? PhoneNumber { get; set; }

    public List<DocumentDto> Documents { get; set; }
}