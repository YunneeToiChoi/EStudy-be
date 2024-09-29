using System.ComponentModel.DataAnnotations;

namespace study4_be.Payment.MomoPayment
{
    public class MomoPaymentRequest
    {
        public string SubPartnerCode { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        [Required]
        public long Amount { get; set; } 
        public string OrderId { get; set; }= string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;

    }
}
