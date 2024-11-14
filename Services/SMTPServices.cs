﻿using FirebaseAdmin.Auth;
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
        public string GenerateDocumentPaymentEmailContent(string username, string orderDate, string orderId, string documentName, string invoiceUrl)
        {
            var stamp = _config["Firebase:Stamp"];
            var signatureImage = _config["Firebase:SignatureImage"];
            var logo = _config["Firebase:Logo"]; // default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }");
            sb.AppendLine(".container { background-color: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h3 { color: #333; }");
            sb.AppendLine(".header { text-align: center; }");
            sb.AppendLine(".order-details { margin: 20px 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine("th, td { padding: 10px; text-align: left; border: 1px solid #ddd; }");
            sb.AppendLine("th { background-color: #007BFF; color: white; }");
            sb.AppendLine(".footer { margin-top: 20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine($"<div class='header'><img src='{logo}' alt='Logo' width='150'/></div>");
            sb.AppendLine($"<p>Xin chào <strong>{username}</strong>, thanh toán đơn hàng của bạn đã thành công vào ngày <strong>{orderDate}</strong></p>");
            sb.AppendLine("<h3>Thông tin đơn hàng:</h3>");
            sb.AppendLine("<div class='order-details'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Mã đơn hàng</th><th>Tên tài liệu</th></tr>");
            sb.AppendLine($"<tr><td>{orderId}</td><td>{documentName}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Cảm ơn bạn đã mua tài liệu của chúng tôi. Chúng tôi hy vọng tài liệu này sẽ hữu ích cho bạn.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine($"<h4>Hóa đơn (INVOICE) : <a href='{invoiceUrl}' target='_blank'>Tải về</a></h4>");
            sb.AppendLine($"<p><img src='{stamp}' alt='Dấu mộc' width='100' /></p>");
            sb.AppendLine($"<p><img src='{signatureImage}' alt='Chữ ký' width='100' /></p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public string GeneratePlanPaymentEmailContent(string username, string orderDate, string orderId, string planName, string invoiceUrl)
        {
            var stamp = _config["Firebase:Stamp"];
            var signatureImage = _config["Firebase:SignatureImage"];
            var logo = _config["Firebase:Logo"]; // default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }");
            sb.AppendLine(".container { background-color: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h3 { color: #333; }");
            sb.AppendLine(".header { text-align: center; }");
            sb.AppendLine(".order-details { margin: 20px 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine("th, td { padding: 10px; text-align: left; border: 1px solid #ddd; }");
            sb.AppendLine("th { background-color: #007BFF; color: white; }");
            sb.AppendLine(".footer { margin-top: 20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine($"<div class='header'><img src='{logo}' alt='Logo' width='150'/></div>");
            sb.AppendLine($"<p>Xin chào <strong>{username}</strong>, bạn đã đăng ký thành công gói dịch vụ vào ngày <strong>{orderDate}</strong></p>");
            sb.AppendLine("<h3>Thông tin đơn hàng:</h3>");
            sb.AppendLine("<div class='order-details'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Mã đơn hàng</th><th>Tên gói dịch vụ</th></tr>");
            sb.AppendLine($"<tr><td>{orderId}</td><td>{planName}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Cảm ơn bạn đã chọn gói dịch vụ của chúng tôi. Chúng tôi hy vọng dịch vụ sẽ mang lại giá trị cho bạn.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine($"<h4>Hóa đơn (INVOICE) : <a href='{invoiceUrl}' target='_blank'>Tải về</a></h4>");
            sb.AppendLine($"<p><img src='{stamp}' alt='Dấu mộc' width='100' /></p>");
            sb.AppendLine($"<p><img src='{signatureImage}' alt='Chữ ký' width='100' /></p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public string GenerateCodeByEmailContent(string username, string orderDate, string orderId, string nameCourse, string codeActiveCourse, string invoiceUrl)
        {
            var stamp = _config["Firebase:Stamp"];
            var signatureImage = _config["Firebase:SignatureImage"];
            var logo = _config["Firebase:Logo"]; // default img
            var sb = new StringBuilder();
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }");
            sb.AppendLine(".container { background-color: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }");
            sb.AppendLine("h3 { color: #333; }");
            sb.AppendLine(".header { text-align: center; }");
            sb.AppendLine(".order-details { margin: 20px 0; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine("th, td { padding: 10px; text-align: left; border: 1px solid #ddd; }");
            sb.AppendLine("th { background-color: #007BFF; color: white; }");
            sb.AppendLine(".footer { margin-top: 20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine($"<div class='header'><img src='{logo}' alt='Logo' width='150'/></div>");
            sb.AppendLine($"<p>Xin chào <strong>{username}</strong>, đơn hàng của bạn đã đặt thành công vào ngày <strong>{orderDate}</strong></p>");
            sb.AppendLine("<h3>Thông tin đơn hàng:</h3>");
            sb.AppendLine("<div class='order-details'>");
            sb.AppendLine("<table>");
            sb.AppendLine("<tr><th>Mã đơn hàng</th><th>Tên khoá học</th><th>Mã kích hoạt</th></tr>");
            sb.AppendLine($"<tr><td>{orderId}</td><td>{nameCourse}</td><td>{codeActiveCourse}</td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("</div>");
            sb.AppendLine("<p>Xin chân thành cảm ơn và chúc quý học viên có một khoá học thành công và hiệu quả.</p>");
            sb.AppendLine("<p>Trân trọng,</p>");
            sb.AppendLine("<p>Đội ngũ EStudy</p>");
            sb.AppendLine("<div class='footer'>");
            sb.AppendLine("<h4>Liên hệ hỗ trợ:</h4>");
            sb.AppendLine("<ul>");
            sb.AppendLine("<li>Email: contact.nangphanvan@gmail.com</li>");
            sb.AppendLine("<li>Số điện thoại: (+84) 902250149</li>");
            sb.AppendLine("</ul>");
            sb.AppendLine($"<h4>Hóa đơn (INVOICE) : <a href='{invoiceUrl}' target='_blank'>Tải về</a></h4>");
            sb.AppendLine($"<p><img src='{stamp}' alt='Dấu mộc' width='100' /></p>");
            sb.AppendLine($"<p><img src='{signatureImage}' alt='Chữ ký' width='100' /></p>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");
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