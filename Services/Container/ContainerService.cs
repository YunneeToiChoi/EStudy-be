
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Errors.Model;
using study4_be.Interface.Rating;
using study4_be.Models;
using study4_be.Models.DTO;
using study4_be.Services.Course;
using study4_be.Services.Document;
using study4_be.Services.Rating;
using study4_be.Services.User;
using study4_be.Services;
using study4_be.Services.Container;
using study4_be.Services.Lesson;
using study4_be.Services.Unit;

namespace study4_be.Services.Rating
{
    public class ContainerService : IContainerService
    {
        private readonly Study4Context _context;
        public ContainerService(Study4Context context)
        {
            _context = context;
        }
        public async Task<UnitDetailResponse> GetAllContainerAndLessonByUnitAsync(GetAllContainerAndLessionRequestcs unit)
        {
            // Get the unit by ID
            var unitExist = await _context.Units.FindAsync(unit.unitId);
            if (unitExist == null)
            {
                throw new NotFoundException("Unit not found");
            }

            // Get the list of containers based on unitId
            var containers = await _context.Containers
                                            .Where(c => c.UnitId == unit.unitId)
                                            .ToListAsync();

            // Loop through each container and get its lessons
            foreach (var container in containers)
            {
                var containerLessons = await _context.Lessons
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

            return unitDetail;
        }
    }
}
