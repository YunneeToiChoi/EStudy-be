using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services;

namespace study4_be.Controllers.Admin
{

    [Route("Admin/CourseManager/[controller]/[action]")]
    public class TagsController : Controller
    {
        private readonly ILogger<TagsController> _logger;
        private readonly Study4Context _context;
        public TagsController(ILogger<TagsController> logger, Study4Context context)
        {
            _logger = logger;
            _context = context;
        }
        

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
        public async Task<IActionResult> Tag_Edit(string id)
        {
            if (!ModelState.IsValid)
            {
                return NotFound(new { message = "tagId is invalid" });
            }
            var tag = await _context.Tags.FirstOrDefaultAsync(c => c.TagId == id);
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
                var courseToUpdate = await _context.Tags.FirstOrDefaultAsync(c => c.TagId == tag.TagId);
                courseToUpdate.TagId = tag.TagId;
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Tag_List");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating tag");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the tag.");
                }
            }
            return View(tag);
        }

        [HttpGet]
        public async Task<IActionResult> Tag_Delete(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Staff not found for deletion.");
                return NotFound($"Staff not found.");
            }
            var tag = await _context.Tags.FirstOrDefaultAsync(c => c.TagId == id);
            if (tag == null)
            {
                _logger.LogError($"Tag not found for delete.");
                return NotFound($"Tag not found.");
            }
            return View(tag);
        }

        [HttpPost, ActionName("Tag_Delete")]
        public async Task<IActionResult> Tag_DeleteConfirmed(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError($"Tag not found for deletion.");
                return NotFound($"Tag not found.");
            }
            var tag = await _context.Tags.FirstOrDefaultAsync(c => c.TagId == id);
            if (tag == null)
            {
                _logger.LogError($"Tag not found for deletion.");
                return NotFound($"Tag not found.");
            }

            try
            {
                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction("Tag_List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the tag.");
                return View(tag);
            }
        }

        public async Task<IActionResult> Tag_Details(string id)
        {
            // Check if the ID is invalid (e.g., not positive)
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid Tag ID.");
                TempData["ErrorMessage"] = "The specified Tag was not found.";
                return RedirectToAction("Tag_List", "Tag");
            }

            var tag = await _context.Tags.FirstOrDefaultAsync(c => c.TagId == id);

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
