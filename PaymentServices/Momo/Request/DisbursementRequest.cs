

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace study4_be.PaymentServices.Momo.Request
{
    public class DisbursementRequest
    {
        public string RequestId { get; set; } // ID yêu cầu duy nhất
        public long Amount { get; set; } // Số tiền giải ngân
        public string OrderId { get; set; } // Mã đơn hàng
        public string PartnerClientId { get; set; } // ID khách hàng của partner
        public string Lang { get; set; } // Ngôn ngữ (VD: "vi" hoặc "en")
        public string RequestType { get; set; }
        public string OrderInfo { get; set; }
        public string ExtraData { get; set; }
        public string userId { get; set; }
        public string walletId { get; set; }
    }
}
