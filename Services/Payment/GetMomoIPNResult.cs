using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Payment
{
    public class GetMomoIPNResult
    {
        public string? partnerCode { get; set; } = string.Empty;
        public string? orderId { get; set; } = string.Empty;
        public string? requestId { get; set; } = string.Empty;
        [Required]
        public required long amount { get; set; }
        public string? orderInfo { get; set; } = string.Empty;
        public string? orderType { get; set; } = string.Empty;
        public string? transId { get; set; } = string.Empty;
        public string? message { get; set; } = string.Empty;
        [Required]
        public required int resultCode { get; set; }
        public string? payType { get; set; } = string.Empty;
        public long? responseTime { get; set; }
        public string? extraData { get; set; } = string.Empty;
        public string? signature { get; set; } = string.Empty;
    }
}
