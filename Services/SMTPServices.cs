using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using study4_be.Models;
using MailKit.Net.Smtp;
using System.Configuration;
using MailKit.Security;

namespace study4_be.Services
{
    public class SMTPServices
    {
        private readonly IConfiguration _config;
        private readonly SmtpServicesAccountKey _serviceAccountKey;
        private readonly string _firebaseBucketName;
        public SMTPServices(IConfiguration config)
        {
            _config = config;
            _serviceAccountKey = _config.GetSection("Smtp").Get<SmtpServicesAccountKey>();
        }
        public string GenerateCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public async Task SendEmailAsync(string recipientEmail, string subject, string plainTextContent, string htmlContent)
        {
            try
            {
                var message = new MimeMessage();

                // Validate sender's email address configuration
                string senderName = "Support"; // Or any other appropriate sender name
                string senderEmailAddress = _config["Smtp:Email"]; // Corrected key "UserName"
                if (string.IsNullOrEmpty(senderEmailAddress))
                {
                    throw new ConfigurationException("SMTP sender email address configuration is missing or invalid.");
                }
                message.From.Add(new MailboxAddress(senderName, senderEmailAddress));

                // Validate recipient's email address
                if (!IsValidEmail(recipientEmail))
                {
                    throw new ArgumentException("Invalid recipient email address.", nameof(recipientEmail));
                }
                message.To.Add(new MailboxAddress("", recipientEmail));

                message.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = plainTextContent,
                    HtmlBody = htmlContent
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_config["Smtp:Email"], _config["Smtp:Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Handle other exceptions (e.g., MimeKit exceptions) appropriately
                throw new Exception($"An error occurred while sending email: {ex.Message}", ex);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
