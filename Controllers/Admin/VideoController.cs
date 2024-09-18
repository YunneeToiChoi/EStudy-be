using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Models.ViewModel;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    public class VideoController : Controller
    {
        private readonly ILogger<VideoController> _logger;
        public VideoController(ILogger<VideoController> logger)
        {
            _logger = logger;
        }
        private readonly VideoRepository _videosRepository = new VideoRepository();
        public Study4Context _context = new Study4Context();
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

                return View(videoViewModels);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error occurred while fetching vocabulary list.");

                // Handle the exception gracefully
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(new List<VocabListViewModel>());
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
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                videoViewMode.Lessons = _context.Lessons.Select(c => new SelectListItem
                {
                    Value = c.LessonId.ToString(),
                    Text = c.LessonTitle.ToString()
                }).ToList();

                return View(videoViewMode);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null)
            {
                return NotFound();
            }

            return Ok(video);
        }

        [HttpGet]
        public IActionResult Video_Edit(int id)
        {
            var video = _context.Videos.FirstOrDefault(c => c.VideoId == id);
            if (video == null)
            {
                return NotFound();
            }
            var lessons = _context.Lessons.ToList();
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
                var courseToUpdate = _context.Videos.FirstOrDefault(c => c.VideoId == videoViewModel.video.VideoId);
                courseToUpdate.VideoUrl = videoViewModel.video.VideoUrl;
                courseToUpdate.LessonId = videoViewModel.video.LessonId;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Video_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {videoViewModel.video.VideoId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(videoViewModel);
        }

        [HttpGet]
        public IActionResult Video_Delete(int id)
        {
            var video = _context.Videos.FirstOrDefault(c => c.VideoId == id);
            if (video == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(video);
        }

        [HttpPost, ActionName("Video_Delete")]
        public IActionResult Video_DeleteConfirmed(int id)
        {
            var video = _context.Videos.FirstOrDefault(c => c.VideoId == id);
            if (video == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Videos.Remove(video);
                _context.SaveChanges();
                return RedirectToAction("Video_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(video);
            }
        }

        public IActionResult Video_Details(int id)
        {
            return View(_context.Videos.FirstOrDefault(c => c.VideoId == id));
        }
    }
}
