using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

using study4_be.Services;


namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AzureOpenAiService _azureOpenAiService;

        public ChatController(AzureOpenAiService azureOpenAiService)
        {
            _azureOpenAiService = azureOpenAiService;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> Generate()
        { 
            int minScore = 0;
            int maxScore = 10;
            var content =
                "During our recent weekend getaway, my friend and I intended to embark on a hiking adventure in the mountains. We were excited about the chance to bond and enjoy the beauty of nature together. Unfortunately, upon reaching the trailhead, we discovered that the weather was not favorable; heavy rain began to pour, and we realized that we hadn't packed sufficient gear to keep ourselves dry. Consequently, we made the difficult choice to cancel our hike and return home, feeling a bit disheartened but understanding that safety was our priority.";
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
            var input = response.ToString();
            // Sử dụng Regex để tách các phần với regex đơn giản và phù hợp
            var scoreMatch = Regex.Match(input, @"\[SCORE\] : (\d+)");
            var commentMatch = Regex.Match(input, @"\[COMMENT\] : ([^\[]+)");
            var explanMatch = Regex.Match(input, @"\[EXPLAN\] : (.+)", RegexOptions.Singleline);

            if (scoreMatch.Success && commentMatch.Success && explanMatch.Success)
            {
                int score = int.Parse(scoreMatch.Groups[1].Value);
                string comment = commentMatch.Groups[1].Value.Trim();
                string explanation = explanMatch.Groups[1].Value.Trim();

                Console.WriteLine($"Score: {score}");
                Console.WriteLine($"Comment: {comment}");
                Console.WriteLine($"Explanation: {explanation}");
            }
            else
            {
                Console.WriteLine("Không thể tách được một hoặc nhiều thành phần.");
            }
            return Ok(response);
        }
        public class Prompt
        {
            public string prompt { get; set; }
        }
    }
}
