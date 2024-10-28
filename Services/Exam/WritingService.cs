using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using study4_be.Interface;

namespace study4_be.Services.Exam;

public class WritingService : IWritingService
{
    private readonly AzureOpenAiService _azureOpenAiService;
    
    public WritingService(AzureOpenAiService azureOpenAiService){ _azureOpenAiService = azureOpenAiService; }

    public async Task<WritingScore> ScoringWritingAsync(int maxScore, string content)
    {
        int minScore = 0;
        int score;
        string comment;
        string explanation;
        var requestContent = @$"
                Please check the following sentence for grammatical errors and provide a corrected version. 
                Also, rate the quality of the writing on a scale from {minScore} to {maxScore}, 
                and provide the response in the following format: 
                [SCORE] : value
                [COMMENT] : value
                [EXPLAN] : value
                Here is the sentence: 
                '{content}'
                ";
        var response = await _azureOpenAiService.GenerateResponseAsync(requestContent);
        // Sử dụng Regex để tách các phần với regex đơn giản và phù hợp
        var scoreMatch = Regex.Match(response, @"\[SCORE\] : (\d+)");
        var commentMatch = Regex.Match(response, @"\[COMMENT\] : ([^\[]+)");
        var explanMatch = Regex.Match(response, @"\[EXPLAN\] : (.+)", RegexOptions.Singleline);

        if (scoreMatch.Success && commentMatch.Success && explanMatch.Success)
        {
            score = int.Parse(scoreMatch.Groups[1].Value);
            comment = commentMatch.Groups[1].Value.Trim();
            explanation = explanMatch.Groups[1].Value.Trim();

            var writingScore = new WritingScore()
            {
                score = score,
                comment = comment,
                explain = explanation
            };
            return writingScore;
        }
        else
        {
            Console.WriteLine("Không thể tách được một hoặc nhiều thành phần.");
            return null;
        }
    }
}