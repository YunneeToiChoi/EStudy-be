namespace study4_be.PaymentServices.Momo.Config
{
    public class MomoConfig
    {
        public static string ConfigName => "Momo";
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PartnerCode { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string AesTokenUrl { get; set; } = string.Empty;
        public string DisbursementUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string Transaction {  get; set; } = string.Empty;
        public string? LinkAccountUrl { get; set; } = string.Empty;
    }
}
