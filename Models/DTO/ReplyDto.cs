using iText.IO.Image;

namespace study4_be.Models.DTO
{
    public class ReplyDto
    {
        public int ReplyId { get; set; }
        public int RatingId { get; set; }
        public string ReplyContent { get; set; }
        public DateTime ReplyDate { get; set; }
        public int? ParentReplyId { get; set; }
        public bool ReplyExist { get; set; }
        public UserDto User { get; set; }
        public List<ImageDto> Images { get; set; }
    }
}