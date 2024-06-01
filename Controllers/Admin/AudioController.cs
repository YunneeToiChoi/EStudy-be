using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class AudioController : Controller
    {
        private readonly ILogger<AudioController> _logger;
        public AudioController(ILogger<AudioController> logger)
        {
            _logger = logger;
        }
        private readonly AudioRepository _audiosRepository = new AudioRepository();
        public STUDY4Context _context = new STUDY4Context();
        [HttpGet("GetAllAudios")]
        public async Task<ActionResult<IEnumerable<Audio>>> GetAllAudios()
        {
            var audios = await _audiosRepository.GetAllAudiosAsync();
            return Json(new { status = 200, message = "Get Audios Successful", audios });

        }
        [HttpDelete("DeleteAllAudios")]
        public async Task<IActionResult> DeleteAllAudios()
        {
            await _audiosRepository.DeleteAllAudiosAsync();
            return Json(new { status = 200, message = "Delete Audios Successful" });
        }
        public IActionResult Audio_List()
        {
            return View();
        }
        public IActionResult Audio_Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Audio_Create(Audio audio)
        {
            if (!ModelState.IsValid)
            {

                return View(audio);    //show form with value input and show errors
            }
            try
            {
                try
                {
                    await _context.AddAsync(audio);
                    await _context.SaveChangesAsync();
                    CreatedAtAction(nameof(GetAudioById), new { id = audio.AudioId }, audio);
                }
                catch (Exception e)
                {
                    CreatedAtAction(nameof(GetAudioById), new { id = audio.AudioId }, audio);
                    _logger.LogError(e, "Error occurred while creating new audio.");
                }
                return RedirectToAction("Index", "Home"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new audio.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(audio);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAudioById(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null)
            {
                return NotFound();
            }

            return Ok(audio);
        }

        public IActionResult Audio_Delete()
        {
            return View();
        }
        public IActionResult Audio_Edit()
        {
            return View();
        }
        public IActionResult Audio_Details()
        {
            return View();
        }
    }
}
