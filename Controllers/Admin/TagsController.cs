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
        private readonly Study4Context _context = new Study4Context();

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
                    _logger.LogError(e, "Error occurred while creating new tag.");
                }
                return RedirectToAction("Tag_List", "Tags"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new tag.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(tag);
            }
        }

        public async Task<IActionResult> GetTagById(int id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "Id is invalid" });
            }
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
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "tagId is invalid" });
            }
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
                    _logger.LogError($"Error updating tag: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the tag.");
                }
            }
            return View(tag);
        }

        [HttpGet]
        public IActionResult Tag_Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }
            var tag = _context.Tags.FirstOrDefault(c => c.TagId == id);
            if (tag == null)
            {
                _logger.LogError($"Tag not found for delete.");
                return NotFound($"Tag not found.");
            }
            return View(tag);
        }

        [HttpPost, ActionName("Tag_Delete")]
        public IActionResult Tag_DeleteConfirmed(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Tag not found for deletion.");
                return NotFound($"Tag not found.");
            }
            var tag = _context.Tags.FirstOrDefault(c => c.TagId == id);
            if (tag == null)
            {
                _logger.LogError($"Tag not found for deletion.");
                return NotFound($"Tag not found.");
            }

            try
            {
                _context.Tags.Remove(tag);
                _context.SaveChanges();
                return RedirectToAction("Tag_List");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting tag: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the tag.");
                return View(tag);
            }
        }

        public IActionResult Tag_Details(string id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Tag ID.");
                TempData["ErrorMessage"] = "The specified Tag was not found.";
                return RedirectToAction("Tag_List", "Tag");
            }

            var tag = _context.Tags.FirstOrDefault(c => c.TagId == id);

            // If no container is found, return to the list with an error
            if (tag == null)
            {
                TempData["ErrorMessage"] = "The specified Tag was not found.";
                return RedirectToAction("Tag_List", "Tag");
            }
            return View(tag);
        }
    }
}
