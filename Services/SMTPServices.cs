using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using study4_be.Models;
using MailKit.Net.Smtp;
using System.Configuration;
using MailKit.Security;
using System.Text;
using MailKit.Search;

namespace study4_be.Services
{
    public class SMTPServices
    {
        private readonly IConfiguration _config;
        private readonly string _firebaseBucketName;
        public SMTPServices(IConfiguration config)
        {
            _config = config;
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
                throw new InvalidOperationException($"An error occurred while sending email: {ex.Message}", ex);
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
        public string GenerateCodeByEmailContent(string username, string orderDate, string orderId, string nameCourse, string codeActiveCourse, string contractUrl)
        {
            var logo = _config["Firebase:Logo"];//default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div style='text-align: center;'>");
            sb.AppendLine($"<img src='{logo}' alt='Logo' width='150'/>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<p>Xin chào {username}, đơn hàng của bạn đã đặt thành công vào ngày {orderDate}</p>");
            sb.AppendLine("<h3>Thông tin đơn hàng:</h3>");
            sb.AppendLine("<ul>");
            sb.AppendLine($"<li>Mã đơn hàng: {orderId}</li>");
            sb.AppendLine($"<li>Tên khoá học: {nameCourse}</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; width: 200px; margin: 0 auto;'>");
            sb.AppendLine($"<p style='text-align: center;'> {codeActiveCourse} </p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Xin chân thành cảm ơn và chúc quý học viên có một khoá học thành công và hiệu quả.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine($"<h4>Hợp đồng (PO): <a href='{contractUrl}' target='_blank'>Tải về</a></h4>");

            // Thêm hình ảnh dấu mộc và chữ ký
            sb.AppendLine("<p><img src='URL_TO_SEAL_IMAGE' alt='Dấu mộc' width='100' /></p>"); // fix header
            sb.AppendLine("<p><img src='URL_TO_SIGNATURE_IMAGE' alt='Chữ ký' width='100' /></p>"); // fix header

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        } 
        public string GenerateCodeByEmailContent(string username, string orderDate, string orderId, string nameCourse, string codeActiveCourse)
        {
            var logo = _config["Firebase:Logo"];//default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div style='text-align: center;'>");
            sb.AppendLine($"<img src='{logo}' alt='Logo' width='150'/>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<p>Xin chào {username}, đơn hàng của bạn đã đặt thành công vào ngày {orderDate}</p>");
            sb.AppendLine("<h3>Thông tin đơn hàng:</h3>");
            sb.AppendLine("<ul>");
            sb.AppendLine($"<li>Mã đơn hàng: {orderId}</li>");
            sb.AppendLine($"<li>Tên khoá học: {nameCourse}</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; width: 200px; margin: 0 auto;'>");
            sb.AppendLine($"<p style='text-align: center;'> {codeActiveCourse} </p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Xin chân thành cảm ơn và chúc quý học viên có một khoá học thành công và hiệu quả.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149</li>");
            sb.AppendLine("</ul>");

            // Thêm hình ảnh dấu mộc và chữ ký
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public string GenerateLinkVerifiByEmailContent(string userEmail, string link)
        {
            var logo = _config["Firebase:Logo"];//default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div style='text-align: center;'>");
            sb.AppendLine($"<img src='{logo}' alt='Logo' width='150'/>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<p>Xin chào {userEmail}, Link xác nhận của bạn là</p>");
            sb.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; width: 200px; margin: 0 auto;'>");
            sb.AppendLine($"<p style='text-align: center;'>{link}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Xin chân thành cảm ơn và chúc quý học viên có một trải nghiệm và học tập thật hiệu quả.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149 </li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        } 
        public string GenerateLinkToResetPassword(string userEmail, string link)
        {
            var logo = _config["Firebase:Logo"];//default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div style='text-align: center;'>");
            sb.AppendLine($"<img src='{logo}' alt='Logo' width='150'/>");
            sb.AppendLine("</div>");
            sb.AppendLine($"<p>Xin chào {userEmail}, Link thay đổi mật khẩu của bạn là</p>");
            sb.AppendLine("<div style='border: 1px solid #ccc; padding: 10px; width: 200px; margin: 0 auto;'>");
            sb.AppendLine($"<p style='text-align: center;'>{link}</p>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Xin chân thành cảm ơn và chúc quý học viên có một trải nghiệm và học tập thật hiệu quả.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149 </li>");
            sb.AppendLine("</ul>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }
    }
}
