using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Services.Document
{
    public class UploadDetailRequest
    {
        public IEnumerable<int> idDocuments { get; set; }
        public string userId { get; set; }
        public int? categoryId { get; set; }
        public int? courseId { get; set; }
        [JsonRequired]
        public bool state { get; set; } // state right here is premium or premium => required post api 
        public string? description { get; set; }
        public double? price { get; set; }
        public string title { get; set; }
    }
}
