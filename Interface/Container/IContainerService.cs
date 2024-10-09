using study4_be.Models.DTO;
using study4_be.Services.Lesson;
using study4_be.Services.Rating;
using study4_be.Services.Unit;

namespace study4_be.Interface.Rating
{
    public interface IContainerService
    {
        Task<UnitDetailResponse> GetAllContainerAndLessonByUnitAsync(GetAllContainerAndLessionRequestcs unit);
    }
}
