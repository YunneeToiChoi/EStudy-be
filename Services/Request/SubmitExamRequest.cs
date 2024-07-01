namespace study4_be.Services.Request
{
    public class SubmitExamRequest
    {
        public string examId { get;set; }
        public string userId { get;set; }
        public int? score { get;set; }
    }
}
