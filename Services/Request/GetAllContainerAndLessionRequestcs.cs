using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class GetAllContainerAndLessionRequestcs
    {
        [Required]
        public required int unitId { get; set; }
    }
}
