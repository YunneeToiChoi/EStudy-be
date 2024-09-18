namespace study4_be.Services.Request
{
    public class OrderPlanRequest
    {
        public string? UserId { get; set; }
        public int PlanId { get; set; }
        public double? Price { get; set; }
        public int Duration { get; set; }
    }
}
