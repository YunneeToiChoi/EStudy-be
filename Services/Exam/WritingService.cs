using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using study4_be.Interface;

namespace study4_be.Services.Exam;

public class WritingService : IWritingService
{
    private readonly AzureOpenAiService _azureOpenAiService;
    
    public WritingService(AzureOpenAiService azureOpenAiService){ _azureOpenAiService = azureOpenAiService; }

    public async Task<WritingScore> ScoringWritingAsync(int maxScore, string content, int modelIndex = 0)
    {
        int minScore = 0;
        int score;
        string comment;
        string explanation;
        var requestContent = @$"
            Please thoroughly evaluate the following sentence for grammatical errors, coherence, style, and overall impact. 
            Provide a corrected version and rate the quality of the writing on a scale from {minScore} to {maxScore} only integer. 
            Include specific feedback on the following aspects: 
            - Grammar
            - Clarity
            - Style and Tone
            - Overall Impact
            Present the response in the following format: 
            [SCORE]: value
            [COMMENT]: value
            [EXPLAIN]: value
            Here is the sentence: 
            '{content}'
        ";

        // Sử dụng model dựa trên tham số modelIndex
        var response = await _azureOpenAiService.GenerateResponseAsync(requestContent, modelIndex);

        var scoreMatch = Regex.Match(response, @"\[SCORE\]: (\d+)"); // Support for decimal scores
        var commentMatch = Regex.Match(response, @"\[COMMENT\]: ([^\[]+?)(?=\[EXPLAIN\]:|\z)");
        var explainMatch = Regex.Match(response, @"\[EXPLAIN\]:([\s\S]+?)$"); // Match everything after [EXPLAIN]:

        if (scoreMatch.Success && commentMatch.Success && explainMatch.Success)
        {
            score = int.Parse(scoreMatch.Groups[1].Value);
            comment = commentMatch.Groups[1].Value.Trim();
            explanation = explainMatch.Groups[1].Value.Trim();

            return new WritingScore()
            {
                score = score,
                comment = comment,
                explain = explanation
            };
        }
        else
        {
            Console.WriteLine("Unable to extract one or more components.");
            return null;
        }
    }


}