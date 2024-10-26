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
namespace study4_be.PaymentServices.Momo.Response
{
    public class TokenizationResponse
    {
        public string PartnerCode { get; set; }
        public string RequestId { get; set; }
        public string OrderId { get; set; }
        public string AesToken { get; set; }
        public int ResultCode { get; set; }
        public string PartnerClientId { get; set; }
        public long ResponseTime { get; set; }
        public string Message { get; set; }
    }
}
