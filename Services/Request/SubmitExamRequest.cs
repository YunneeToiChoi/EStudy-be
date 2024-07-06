namespace study4_be.Services.Request
{
    public class SubmitExamRequest
    {
        public string examId { get;set; }
        public string userId { get;set; }
        public int? score { get;set; }
        public List<AnswerDto> answer { get; set; }

    }
    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
    }
}
