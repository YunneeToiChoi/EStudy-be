
namespace study4_be.PaymentServices.Momo.Request
{
    public class TokenizationRequest
    {
        public string PartnerCode { get; set; }
        public string RequestId { get; set; }
        public string CallbackToken { get; set; }
        public string OrderId { get; set; }
        public string PartnerClientId { get; set; }
        public string Lang { get; set; }
        public string Signature { get; set; }
    }

}
