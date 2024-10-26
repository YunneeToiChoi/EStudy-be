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
            var requestContent = $"Please check the following sentence for grammatical errors and provide a corrected version. " +
                                 $"Also, rate the quality of the writing on a scale from {minScore} to {maxScore}, " +
                                 $"and provide the response in the following format: \n" +
                                 $"[SCORE] : value\n" +
                                 $"[COMMENT] : value\n" +
                                 $"[EXPLAN] : value\n" +
                                 $"Here is the sentence: \n'{content}'";
            var response = await _azureOpenAiService.GenerateResponseAsync(requestContent);
            return Ok(response);
        }
        public class Prompt
        {
            public string prompt { get; set; }
        }
    }
}
