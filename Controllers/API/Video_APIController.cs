using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Video_APIController : Controller
    {
        public Study4Context _context;

        public Video_APIController(Study4Context context) { _context = context; }

        [HttpPost("Get_AllVideoOfLesson")]
        public async Task<IActionResult> Get_AllVideoOfLesson(OfLessonRequest _req)
        {
            var video = _context.Videos.Where(v => v.LessonId == _req.lessonId);
            var lessonTag = await _context.Lessons
                .Where(l => l.LessonId == _req.lessonId)
                .Select(l => l.Tag)
                .FirstAsync();
            var lessonTagResponse = new
            {
                lessonTag = lessonTag.TagId
            };
            return Json(new { status = 200, message = "Get All Video of lesson Successful", data = video, lessonTag = lessonTagResponse});
        }
    }
}
