namespace study4_be.Services.Response
{
    public class QuestionDoubleChoice
    {
        public int QuestionId { get; set; }
        public string? QuestionTitle { get; set; }
        public string? Title_Mean { get; set; }
        public string? QuestionTranslate { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? A_Mean { get; set; }
        public string? B_Mean { get; set; }
    }
}
