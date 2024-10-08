namespace study4_be.Services.Plan
{
    public class PlanResponse
    {
        public int PlanId { get; set; }

        public string? PlanName { get; set; }

        public double? PlanPrice { get; set; }

        public int? PlanDuration { get; set; }

        public string? PlanDescription { get; set; }
    }
}
