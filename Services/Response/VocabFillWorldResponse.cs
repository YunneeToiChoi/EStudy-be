namespace study4_be.Services.Response
{
    public class VocabFillWorldResponse
    {
        public int vocabId { get; set; }
        public string vocabTitle { get; set; } = string.Empty;
        public string vocabMean { get; set; } = string.Empty;
        public string vocabExplanation { get; set; } = string.Empty;
        public string vocabExmample { get; set; } = string.Empty;   
        public int typeAgothism { get; set; }
    }
}
