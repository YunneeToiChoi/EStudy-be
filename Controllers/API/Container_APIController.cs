using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using study4_be.Models;
using study4_be.Repositories;
using study4_be.Services.Request;
using study4_be.Services.Response;

namespace study4_be.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class Container_APIController : Controller
    {
        private readonly ILogger<Container_APIController> _logger;
        public ContainerRepository _containerRepo = new ContainerRepository();
        public Container_APIController(ILogger<Container_APIController> logger)
        {
            _logger = logger;
        }
        public Study4Context _context = new Study4Context();
        [HttpPost("Get_AllContainerAndLessonByUnit")]
        public async Task<ActionResult<UnitDetailResponse>> Get_AllContainerAndLessonByUnit(GetAllContainerAndLessionRequestcs unit)
        {
            using (var context = new Study4Context())
            {
                // Get the unit by ID
                var unitExist = await context.Units.FindAsync(unit.unitId);
                if (unitExist == null)
                {
                    return NotFound(new { status = 404, message = "Unit not found" });
                }

                // Get the list of containers based on unitId
                var containers = await context.Containers
                                              .Where(c => c.UnitId == unit.unitId)
                                              .ToListAsync();

                // Loop through each container and get its lessons
                foreach (var container in containers)
                {
                    var containerLessons = await context.Lessons
                                                        .Where(l => l.ContainerId == container.ContainerId)
                                                        .ToListAsync();
                    container.Lessons = containerLessons;
                }

                // Create the response object
                var unitDetail = new UnitDetailResponse
                {
                    unitId = unit.unitId,
                    unitName = unitExist.UnitTittle,
                    Containers = containers.Select(c => new ContainerResponse
                    {
                        ContainerId = c.ContainerId,
                        ContainerTitle = c.ContainerTitle,
                        Lessons = c.Lessons.Select(l => new LessonResponse
                        {
                            LessonId = l.LessonId,
                            LessonTitle = l.LessonTitle,
                            LessonType = l.LessonType,
                            tagId = l.TagId,    
                        }).ToList()
                    }).ToList()
                };

                // Return the formatted response
                return Ok(new
                {
                    status = 200,
                    message = "Get All Units and Containers Successful",
                    unitDetail
                });
            }
        }
    }
}
