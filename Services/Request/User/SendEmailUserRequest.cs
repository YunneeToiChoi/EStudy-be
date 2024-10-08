using Org.BouncyCastle.Bcpg.OpenPgp;

namespace study4_be.Services.Request.User
{
    public class SendEmailUserRequest
    {
        public string? userEmail { get; set; } = string.Empty;
    }
}
