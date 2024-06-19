using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Microsoft.AspNetCore.Mvc;

namespace study4_be.Services
{
    public class SMTPServices
    {
      
        public string GenerateCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task SendEmailUsingSendGrid(string recipientEmail, string emailLink)
        {
            // Example: Sending email using SendGrid
            // Replace with your actual email sending code (using SMTP, SendGrid, etc.)
            // This is just a placeholder example; you need to implement actual email sending code
            // Here's an example using SendGrid (you'll need to install SendGrid NuGet package):
            /*
            var client = new SendGridClient("your_sendgrid_api_key");
            var from = new EmailAddress("sender@example.com", "Sender Name");
            var subject = "Email verification link";
            var to = new EmailAddress(recipientEmail);
            var plainTextContent = $"Please verify your email by clicking on this link: {emailLink}";
            var htmlContent = $"<strong>Please verify your email by clicking on this link: <a href='{emailLink}'>Verify Email</a></strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            */
        }
    }
}
