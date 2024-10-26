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
            var content = "Please check the following sentence for grammatical errors and provide a corrected version. Also, rate the quality of the writing on a scale from 0 to 5, and provide the response in the following format: SCORE : value\r \nCOMMENT : value\r \nEXPLAN : value \nHere is the sentence: \n'Last weekend, me and my friend was planning to go on a hiking trip in the mountains. We thought it would be a great opportunity for us to spend time together and enjoy nature. However, when we arrived at the trail, we realized that the weather was not what we expected, it started raining heavily and we didn’t brought enough equipment to stay dry. So, we decided to cancel our trip and went back home instead, feeling disappointed but also knowing that it was for the best.'";
            var response = await _azureOpenAiService.GenerateResponseAsync(content);
            return Ok(response);
        }
        public class Prompt
        {
            public string prompt { get; set; }
        }
    }
}
