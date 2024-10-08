namespace study4_be.Services.Response.Question
{
    public class VocabFindPairResponse
    {
        public int vocabId { get; set; }
        public string vocabTitle { get; set; } = string.Empty;
        public string vocabMean { get; set; } = string.Empty;
        public string vocabExplanation { get; set; } = string.Empty;
    }
}
