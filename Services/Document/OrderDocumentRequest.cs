using System.Text.Json.Serialization;

namespace study4_be.Services.Document
{
    public class OrderDocumentRequest
    {
        [JsonRequired]
        public string userId { get; set; }
        [JsonRequired]
        public int documentId { get; set; }
    }
}
