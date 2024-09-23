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
                    using (PdfWriter writer = new PdfWriter(memoryStream))
                    {
                        PdfDocument pdf = new PdfDocument(writer);
                        var document = new iText.Layout.Document(pdf);
                        string stampImage = _config["Firebase:Stamp"];
                        string signatureImage = _config["Firebase:SignatureImage"];

                        // Thêm tiêu đề hóa đơn
                        document.Add(new Paragraph("HÓA ĐƠN ĐIỆN TỬ")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(20)
                            .SetBold());

                        // Thông tin khách hàng
                        document.Add(new Paragraph($"Tên khách hàng: {user.UserName}").SetFontSize(12));
                        document.Add(new Paragraph($"Mã đơn hàng: {orderId}").SetFontSize(12));
                        document.Add(new Paragraph($"Tên khóa học: {course.CourseName}").SetFontSize(12));
                        document.Add(new Paragraph($"Ngày đặt: {existOrder.OrderDate.ToString()}").SetFontSize(12));
                        document.Add(new Paragraph(" ")); // Khoảng trống

                        // Thêm mộc đỏ
                        Image stamp = new Image(ImageDataFactory.Create(stampImage));
                        stamp.SetWidth(100); // Đặt kích thước mộc đỏ
                        document.Add(stamp);

                        // Thêm chữ ký
                        Image signature = new Image(ImageDataFactory.Create(signatureImage));
                        signature.SetWidth(100); // Đặt kích thước chữ ký
                        document.Add(signature);

                        // Đóng document
                        document.Close();
                    }

                    // Trả về file PDF
                    return new FileContentResult(memoryStream.ToArray(), "application/pdf")
                    {
                        FileDownloadName = $"Invoice_{orderId}.pdf"
                    };
                }
            }
            catch (Exception)
            {
                return new StatusCodeResult(500); // Trả về lỗi 500
            }
        }

    }
}
