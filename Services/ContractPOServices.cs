using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using study4_be.Models;
using iText.IO.Font;
using iText.Kernel.Font;
using System.Globalization;
using Document = iText.Layout.Document;
using iText.Layout.Borders;
using iText.Kernel.Colors;
namespace study4_be.Services
{
    public class ContractPOServices
    {
        private readonly IConfiguration _config;
        private readonly Study4Context _context;
        private readonly FireBaseServices _fireBaseServices;

        public ContractPOServices(IConfiguration config, Study4Context context,FireBaseServices fireBaseServices)
        {
            _config = config;
            _context = context;
            _fireBaseServices = fireBaseServices;
        }
        public async Task<string?> GenerateInvoicePdf(string orderId)
        {
            try
            {
                var existOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (existOrder == null)
                {
                    return null; // Nếu đơn hàng không tồn tại
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == existOrder.UserId);
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == existOrder.CourseId);

                using (var outputStream = new MemoryStream())
                {
                    var writer = new PdfWriter(outputStream);
                    var pdf = new PdfDocument(writer);
                    var document = new iText.Layout.Document(pdf);

                    // Thêm font hỗ trợ tiếng Việt
                    var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "arial-unicode-ms.ttf");
                    var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                    // Tiêu đề hóa đơn
                    document.Add(new Paragraph("HÓA ĐƠN ĐIỆN TỬ")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(font)
                        .SetFontSize(24)
                        .SetBold()
                        .SetMarginBottom(20)
                        .SetFontColor(new DeviceRgb(0, 102, 204))); // Màu xanh

                    // Thông tin khách hàng
                    document.Add(new Paragraph($"Tên khách hàng: {user.UserName}")
                        .SetFont(font)
                        .SetFontSize(14)
                        .SetMarginBottom(5));
                    document.Add(new Paragraph($"Mã đơn hàng: {orderId}")
                        .SetFont(font)
                        .SetFontSize(14)
                        .SetMarginBottom(5));
                    document.Add(new Paragraph($"Tên khóa học: {course.CourseName}")
                        .SetFont(font)
                        .SetFontSize(14)
                        .SetMarginBottom(5));
                    document.Add(new Paragraph($"Ngày đặt: {existOrder.OrderDate:dd/MM/yyyy}")
                          .SetFont(font)
                        .SetFontSize(14)
                        .SetMarginBottom(20));

                    // Thêm bảng
                    Table table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 3, 2 })).SetWidth(UnitValue.CreatePercentValue(100));
                    table.SetMarginTop(20);
                    table.SetMarginBottom(20);
                    table.SetPadding(5);
                    table.SetBorder(Border.NO_BORDER);

                    // Định dạng tiêu đề bảng
                    table.AddHeaderCell(new Cell().Add(new Paragraph("STT").SetFont(font).SetFontSize(12).SetBold())
                        .SetBackgroundColor(new DeviceRgb(0, 102, 204))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Tên sản phẩm").SetFont(font).SetFontSize(12).SetBold())
                        .SetBackgroundColor(new DeviceRgb(0, 102, 204))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER));
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Giá").SetFont(font).SetFontSize(12).SetBold())
                        .SetBackgroundColor(new DeviceRgb(0, 102, 204))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetBorder(Border.NO_BORDER));

                    // Dữ liệu trong bảng
                    table.AddCell(new Cell().Add(new Paragraph("1").SetFont(font).SetFontSize(12)).SetTextAlignment(TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(course.CourseName).SetFont(font).SetFontSize(12)));
                    table.AddCell(new Cell()
                              .Add(new Paragraph($"{existOrder.TotalAmount:C0}")
                              .SetFont(font)
                              .SetFontSize(12))
                              .SetTextAlignment(TextAlignment.RIGHT));


                    // Định dạng viền và padding cho ô trong bảng
                    foreach (var cell in table.GetChildren())
                    {
                        if (cell is Cell tableCell)
                        {
                            tableCell.SetBorder(Border.NO_BORDER);
                            tableCell.SetPadding(5);
                        }
                    }

                    // Thêm bảng vào tài liệu
                    document.Add(table);

                    // Chèn hình ảnh chữ ký và stamp từ Firebase
                    string signatureUrl = _config["Firebase:SignatureImage"];
                    string stampUrl = _config["Firebase:Stamp"];

                    var signatureImage = new Image(ImageDataFactory.Create(signatureUrl))
                        .ScaleAbsolute(100, 50)
                        .SetFixedPosition(400, 100);
                    document.Add(signatureImage);

                    var stampImage = new Image(ImageDataFactory.Create(stampUrl))
                        .ScaleAbsolute(80, 80)
                        .SetFixedPosition(100, 100);
                    document.Add(stampImage);

                    // Đóng tài liệu
                    document.Close();

                    // Sao chép nội dung sang MemoryStream khác để upload
                    using (var uploadStream = new MemoryStream(outputStream.ToArray()))
                    {
                        uploadStream.Seek(0, SeekOrigin.Begin);

                        // Tạo tên file hóa đơn
                        string invoiceFileName = $"{user.UserId}_{orderId}.pdf";

                        // Upload file PDF lên Firebase
                        var pdfFileUrl = await _fireBaseServices.UploadInvoiceAsync(uploadStream, invoiceFileName, user.UserId);

                        return pdfFileUrl; // Trả về URL của file PDF
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                return null; // Trả về null nếu có lỗi xảy ra
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
