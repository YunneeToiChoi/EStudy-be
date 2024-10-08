using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Services.Rating
{
    public class ShowReplyRequest
    {
        [JsonRequired]
        public int ratingId { get; set; }
        public int parentId { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
    }
}
