using Org.BouncyCastle.Asn1.X9;
using static Google.Cloud.Firestore.V1.StructuredAggregationQuery.Types.Aggregation.Types;
using study4_be.Models;

namespace study4_be.Services.Payment
{
    public class BankLinkRequest
    {
        public string SubPartnerCode { get; set; } = string.Empty;
        public string? RequestId { get; set; } = string.Empty;
        public required long Amount { get; set; }
        public string? OrderId { get; set; } = string.Empty;
        public string? OrderInfo { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty; //
        public string partnerClientId { get; set; } //
        public string ExtraData { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;
        public UserInfo UserInfo { get; set; }
    }

    public class UserInfo
    {
        public string? PartnerClientAlias { get; set; } = string.Empty;
        public string UserId { get; set; }
        public string WalletName { get; set; } 
    }

}