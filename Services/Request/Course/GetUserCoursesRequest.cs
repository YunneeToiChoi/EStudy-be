namespace study4_be.Services.Request.Course
{
    public class GetUserCoursesRequest
    {
        public string userId { get; set; } = string.Empty;
        public int amountOutstanding { get; set; } = 4; // Default value set to 0
    }
}
