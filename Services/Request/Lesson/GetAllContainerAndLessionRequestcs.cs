using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request.Lesson
{
    public class GetAllContainerAndLessionRequestcs
    {
        [Required]
        public required int unitId { get; set; }
    }
}
