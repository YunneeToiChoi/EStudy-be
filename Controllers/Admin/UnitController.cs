using Microsoft.AspNetCore.Mvc;
using study4_be.Models;
using study4_be.Repositories;

namespace study4_be.Controllers.Admin
{
    public class UnitController : Controller
    {
        private readonly ILogger<UnitController> _logger;
        public UnitController(ILogger<UnitController> logger)
        {
            _logger = logger;
        }
        private readonly UnitRepository _unitsRepository = new UnitRepository();
        public STUDY4Context _context = new STUDY4Context();

        [HttpGet("GetAllUnits")]
        //public async Task<ActionResult<IEnumerable<Question>>> GetAllUnits()
        //{
        //    need id course
        //    var units = await _unitsRepository.GetAllUnitsByCourseAsync();
        //    return Json(new { status = 200, message = "Get Units Successful", units });

        //}
        //development enviroment
        [HttpDelete("DeleteAllUnits")]
        public async Task<IActionResult> DeleteAllCourses()
        {
            await _unitsRepository.DeleteAllUnitsAsync();
            return Json(new { status = 200, message = "Delete Units Successful" });
        }
        public IActionResult Unit_List()
        {
            return View();
        }
        public IActionResult Unit_Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Unit_Create(Unit unit)
        {
            if (!ModelState.IsValid)
            {

                return View(unit);    //show form with value input and show errors
            }
            try
            {
                try
                {
                    await _context.AddAsync(unit);
                    await _context.SaveChangesAsync();
                    CreatedAtAction(nameof(GetUnitById), new { id = unit.UnitId }, unit);
                }
                catch (Exception e)
                {
                    CreatedAtAction(nameof(GetUnitById), new { id = unit.UnitId }, unit);
                    _logger.LogError(e, "Error occurred while creating new unit.");
                }
                return RedirectToAction("Index", "Home"); // nav to main home when add successfull, after change nav to index create Courses
            }
            catch (Exception ex)
            {
                // show log
                _logger.LogError(ex, "Error occurred while creating new unit.");
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(unit);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUnitById(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }

            return Ok(unit);
        }
        public IActionResult Unit_Delete()
        {
            return View();
        }
        public IActionResult Unit_Edit()
        {
            return View();
        }
        public IActionResult Unit_Details()
        {
            return View();
        }
    }
}
