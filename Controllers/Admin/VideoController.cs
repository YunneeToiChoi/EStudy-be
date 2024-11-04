using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{

    [Route("Admin/CourseManager/[controller]/[action]")]
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        private readonly Study4Context _context;
        public VideoController(ILogger<VideoController> logger, Study4Context context)
        {
            _logger = logger;
            _context = context;
        }
        
        public async Task<IActionResult> Video_List()
        {
            try
            {
                var videos = await _context.Videos
                    .Include(v => v.Lesson)
                    .ThenInclude(l => l.Container)
                    .ThenInclude(c => c.Unit)
                    .ThenInclude(u => u.Course)
                    .ToListAsync();

                var videoViewModels = videos
                    .Select(video => new VideoListViewModel
                    {
                        video = video,
                        courseName = video.Lesson?.Container?.Unit?.Course?.CourseName ?? "N/A",
                        unitTittle = video.Lesson?.Container?.Unit?.UnitTittle ?? "N/A",
                        containerTittle = video.Lesson?.Container?.ContainerTitle ?? "N/A",
                        lessonTittle = video.Lesson?.LessonTitle ?? "N/A",
                    }).ToList();

                return View(videoViewModels); // Make sure this is VideoListViewModel
            }
            catch (Exception ex)
            {
                // Handle exception
                _logger.LogError(ex, "Error occurred while fetching videos.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(new List<VideoListViewModel>()); // This should match the view's model type
            }
        }

        public async Task<IActionResult> Video_Create()
        {
            var lessons = await _context.Lessons
                .Include(l => l.Container)
                .ThenInclude(c => c.Unit)
                    .ThenInclude(u => u.Course)
            .ToListAsync();

            var model = new VideoCreateViewModel
            {
                videos = new Video(),
                Lessons = lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = $"{c.LessonTitle} - Container: {(c.Container?.ContainerTitle ?? "N/A")} - Unit: {(c.Container?.Unit?.UnitTittle ?? "N/A")} - Course: {(c.Container?.Unit?.Course?.CourseName ?? "N/A")} - TAG: {(c.TagId ?? "N/A")}"
                }).ToList()
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Video_Create(VideoCreateViewModel videoViewMode)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Error occurred while creating new videos.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                videoViewMode.Lessons = await _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToListAsync();

                return View(videoViewMode);
            }
            try
            {
                var video = new Video
                {
                    VideoUrl = videoViewMode.videos.VideoUrl,
                    LessonId = videoViewMode.videos.LessonId,
                    VideoId = videoViewMode.videos.VideoId,
                };

                await _context.AddAsync(video);
                await _context.SaveChangesAsync();

                return RedirectToAction("Video_List", "Video");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating new video.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                videoViewMode.Lessons = await _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToListAsync();

                return View(videoViewMode);
            }
        }

        public async Task<IActionResult> GetVideoById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            return Ok(video);
        }

        [HttpGet]
        public async Task<IActionResult> Video_Edit(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "videoId is invalid" });
            }
            var video = await _context.Videos.FirstOrDefaultAsync(c => c.VideoId == id);
            if (video == null)
            {
                return NotFound();
            }
            var lessons = await _context.Lessons.ToListAsync();
            var selectListLessons = lessons.Select(lesson => new SelectListItem
            {
                Value = lesson.LessonId.ToString(),
                Text = lesson.LessonId.ToString()
            }).ToList();

            var viewModel = new VideoEditViewModel
            {
                video = video,
                lesson = selectListLessons
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Video_Edit(VideoEditViewModel videoViewModel)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = await _context.Videos.FirstOrDefaultAsync(c => c.VideoId == videoViewModel.video.VideoId);
                courseToUpdate.VideoUrl = videoViewModel.video.VideoUrl;
                courseToUpdate.LessonId = videoViewModel.video.LessonId;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Video_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating video");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the video.");
                }
            }
            return View(videoViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Video_Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Video not found for deletion.");
                return NotFound($"Video not found.");
            }
            var video = await _context.Videos.FirstOrDefaultAsync(c => c.VideoId == id);
            if (video == null)
            {
                _logger.LogError($"Video not found for delete.");
                return NotFound($"Video not found.");
            }
            return View(video);
        }

        [HttpPost, ActionName("Video_Delete")]
        public async Task<IActionResult> Video_DeleteConfirmed(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Video not found for deletion.");
                return NotFound($"Video not found.");
            }
            var video = await _context.Videos.FirstOrDefaultAsync(c => c.VideoId == id);
            if (video == null)
            {
                _logger.LogError($"Video not found for deletion.");
                return NotFound($"Video not found.");
            }

            try
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
                return RedirectToAction("Video_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting video");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the video.");
                return View(video);
            }
        }

        public async Task<IActionResult> Video_Details(int id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Video ID.");
                TempData["ErrorMessage"] = "The specified Video was not found.";
                return RedirectToAction("Video_List", "Video");
            }

            var video = await _context.Videos.FirstOrDefaultAsync(c => c.VideoId == id);

            // If no container is found, return to the list with an error
            if (video == null)
            {
                TempData["ErrorMessage"] = "The specified video was not found.";
                return RedirectToAction("Video_List", "Video");
            }
            return View(video);
        }
    }
}
