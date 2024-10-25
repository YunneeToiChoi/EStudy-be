namespace study4_be.PaymentServices.Momo.Request
{
    public class DecryptAesTokenRequest
    {
        public string PartnerCode { get; set; }
        public long Amount { get; set; }
        public string CallbackToken { get; set; }
        public string RequestId { get; set; }
        public string OrderId { get; set; }
        public string PartnerClientId { get; set; }
        public string Lang { get; set; }
    }
}
