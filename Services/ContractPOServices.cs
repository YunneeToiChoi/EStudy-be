using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using study4_be.Models;

namespace study4_be.Services
{
    public class ContractPOServices
    {
        private readonly IConfiguration _config;
        private readonly Study4Context _context;

        public ContractPOServices(IConfiguration config, Study4Context context)
        {
            _config = config;
            _context = context;
        }

        public async Task<IActionResult> GenerateInvoicePdf(string orderId)
        {
            try
            {
                // Lấy thông tin đơn hàng từ cơ sở dữ liệu
                var existOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (existOrder == null)
                    return new NotFoundResult();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == existOrder.UserId);
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == existOrder.CourseId);

                using (var memoryStream = new MemoryStream())
                {
                    // Tạo PDF
                    using (PdfWriter writer = new PdfWriter(memoryStream)) // No SmartMode used
                    {
                        PdfDocument pdf = new PdfDocument(writer);
                        var document = new iText.Layout.Document(pdf);
                        string stampImageUrl = _config["Firebase:Stamp"];
                        string signatureImageUrl = _config["Firebase:SignatureImage"];

                        // Add title to the invoice
                        document.Add(new Paragraph("HÓA ĐƠN ĐIỆN TỬ")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20)
                            .SetBold());

                        // Add customer information
                        document.Add(new Paragraph($"Tên khách hàng: {user.UserName}").SetFontSize(12));
                        document.Add(new Paragraph($"Mã đơn hàng: {orderId}").SetFontSize(12));
                        document.Add(new Paragraph($"Tên khóa học: {course.CourseName}").SetFontSize(12));
                        document.Add(new Paragraph($"Ngày đặt: {existOrder.OrderDate.ToString()}").SetFontSize(12));
                        document.Add(new Paragraph(" ")); // Add a blank line for spacing

                        // Load stamp and signature images from Firebase and add them to the document
                        var stampImage = await DownloadImageFromUrlAsync(stampImageUrl);
                        var signatureImage = await DownloadImageFromUrlAsync(signatureImageUrl);

                        if (stampImage != null)
                        {
                            Image stamp = new Image(ImageDataFactory.Create(stampImage));
                            stamp.SetWidth(100); // Set size for stamp
                            document.Add(stamp);
                        }

                        if (signatureImage != null)
                        {
                            Image signature = new Image(ImageDataFactory.Create(signatureImage));
                            signature.SetWidth(100); // Set size for signature
                            document.Add(signature);
                        }

                        // Close the document
                        document.Close();
                    }

                    // Return the PDF file
                    return new FileContentResult(memoryStream.ToArray(), "application/pdf")
                    {
                        FileDownloadName = $"Invoice_{orderId}.pdf"
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                return new StatusCodeResult(500); // Trả về lỗi 500
            }
        }

        // Helper method to download an image from a URL
        private async Task<byte[]> DownloadImageFromUrlAsync(string imageUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(imageUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                }
                catch
                {
                    // Handle any errors related to downloading the image
                }
            }
            return null; // Return null if unable to download
        }


    }
}
