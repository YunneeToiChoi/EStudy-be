using study4_be.Models.DTO;

namespace study4_be.Services.Response
{
    public class ShowReplyResponse
    {
        public int RatingId { get; set; }
        public string RatingContent { get; set; }
        public List<ReplyDto> Replies { get; set; }
    }
}
