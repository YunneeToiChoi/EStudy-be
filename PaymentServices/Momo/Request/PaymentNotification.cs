using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NuGet.ContentModel;
using study4_be.Helper;
using study4_be.PaymentServices.Momo.Response;
using System.Text;

namespace study4_be.PaymentServices.Momo.Request
{
    public class PaymentNotification
    {
        public string PartnerCode { get; set; }
        public string OrderId { get; set; }
        public string RequestId { get; set; }
        public long Amount { get; set; }
        public string OrderInfo { get; set; }
        public string OrderType { get; set; } // Momo_wallet
        public string PartnerClientId { get; set; }
        public string CallbackToken { get; set; }
        public long TransId { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public string PayType { get; set; }
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; }
        public string Signature { get; set; }
    }
}
