namespace study4_be.Services.Response
{
    public class ListenSpeechResponse
    {
        public int QuestionId { get; set; } 
        public string? QuestionTitle { get; set; }   
        public string? Title_Mean { get;set; }
        public string? QuestionAudio { get; set; }
        public string? QuestionParagraph { get; set; }
        public string? Paragraph_Mean { get; set; }
        public string? QuestionTranslate { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? A_Mean { get; set; }
        public string? B_Mean { get; set; }
        public string? C_Mean { get; set; }
        public string? D_Mean { get; set; }
    }
}
