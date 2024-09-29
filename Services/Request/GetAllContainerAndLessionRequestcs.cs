using System.ComponentModel.DataAnnotations;

namespace study4_be.Services.Request
{
    public class GetAllContainerAndLessionRequestcs
    {
        [Required]
        public int unitId { get; set; }
    }
}
