using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{
    public class TagsController : Controller
    {
        private readonly ILogger<TagsController> _logger;
        public TagsController(ILogger<TagsController> logger)
        {
            _logger = logger;
        }
        private Study4Context _context = new Study4Context();

        public async Task<IActionResult> Tag_List()
        {
            var tags = await _context.Tags.ToListAsync(); // Retrieve list of courses from repository
            return View(tags); // Pass the list of courses to the view
        }
        public IActionResult Tag_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Tag_Create(Tag tag)
        {
            if (!ModelState.IsValid)
            {

                return View(tag);    //show form with value input and show errors
            }
            try
            {
                try
                {
                    await _context.AddAsync(tag);
                    await _context.SaveChangesAsync();
                    CreatedAtAction(nameof(GetTagById), new { id = tag.TagId }, tag);
                }
                catch (Exception e)
                {
                    CreatedAtAction(nameof(GetTagById), new { id = tag.TagId }, tag);
                    _logger.LogError(e, "Error occurred while creating new course.");
                }
                return RedirectToAction("Tag_List", "Tags"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new course.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(tag);
            }
        }

        public async Task<IActionResult> GetTagById(int id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        [HttpGet]
        public IActionResult Tag_Edit(string id)
        {
            var tag = _context.Tags.FirstOrDefault(c => c.TagId == id);
            if (tag == null)
            {
                return NotFound();
            }
            return View(tag);
        }

        [HttpPost]
        public async Task<IActionResult> Tag_Edit(Tag tag)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _context.Tags.FirstOrDefault(c => c.TagId == tag.TagId);
                courseToUpdate.TagId = tag.TagId;
                try
                {
                    _context.SaveChanges();
                    return RedirectToAction("Tag_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating course with ID {tag.TagId}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the course.");
                }
            }
            return View(tag);
        }

        [HttpGet]
        public IActionResult Tag_Delete(string id)
        {
            var tag = _context.Tags.FirstOrDefault(c => c.TagId == id);
            if (tag == null)
            {
                _logger.LogError($"Course with ID {id} not found for delete.");
                return NotFound($"Course with ID {id} not found.");
            }
            return View(tag);
        }

        [HttpPost, ActionName("Tag_Delete")]
        public IActionResult Tag_DeleteConfirmed(string id)
        {
            var tag = _context.Tags.FirstOrDefault(c => c.TagId == id);
            if (tag == null)
            {
                _logger.LogError($"Course with ID {id} not found for deletion.");
                return NotFound($"Course with ID {id} not found.");
            }

            try
            {
                _context.Tags.Remove(tag);
                _context.SaveChanges();
                return RedirectToAction("Tag_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting course with ID {id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
                return View(tag);
            }
        }

        public IActionResult Tag_Details(string id)
        {
            return View(_context.Tags.FirstOrDefault(c => c.TagId == id));
        }
    }
}
