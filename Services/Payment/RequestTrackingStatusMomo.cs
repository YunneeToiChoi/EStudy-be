namespace study4_be.Services.Payment
{
    public class RequestTrackingStatusMomo
    {
        public string? partnerCode { get; set; }
        public string? requestId { get; set; }
        public string? orderId { get; set; }
        public string? lang { get; set; }
        public string? signature { get; set; }
    }
}
