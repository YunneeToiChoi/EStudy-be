using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Services.Document
{
    public class OfDocumentIdRequest
    {
        [Required]
        public int documentId { get; set; }
    }
}
