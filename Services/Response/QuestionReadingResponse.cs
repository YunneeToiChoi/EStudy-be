namespace study4_be.Services.Response
{
    public class QuestionReadingResponse
    {
        public int QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public string? QuestionParagraph { get; set; }
        public string? QuestionTranslate { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
    }
}
