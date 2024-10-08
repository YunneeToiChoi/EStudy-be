namespace study4_be.Services.Request.Plan
{
    public class GetUserPlansRequest
    {
        public string userId { get; set; } = string.Empty;
        public int amountOutstanding { get; set; } = 0; // Default value set to 0
    }
}
