using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Container;
using study4_be.Services.Lesson;
using study4_be.Services.Unit;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Container_APIController : ControllerBase
    {
        private readonly IContainerService _containerService;

        public Container_APIController(IContainerService containerService) { _containerService = containerService; }
        [HttpPost("Get_AllContainerAndLessonByUnit")]
        public async Task<ActionResult> Get_AllContainerAndLessonByUnit(GetAllContainerAndLessionRequestcs unit)
        {
            try
            {
                var unitDetail = await _containerService.GetAllContainerAndLessonByUnitAsync(unit);

                return Ok(new
                {
                    status = 200,
                    message = "Get All Units and Containers Successful",
                    unitDetail
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { status = 404, message = ex.Message });
            }
            catch (Exception ex) // General exception handling
            {
                return StatusCode(500, new { status = 500, message = "An unexpected error occurred", error = ex.Message });
            }
        }
    }
}
