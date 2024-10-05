namespace study4_be.Services.Response
{
    public class RatingCourseResponse
    {
        public int ratingId { get; set; }
        public string userId { get; set; } = null!;
        public string userImage { get; set; } = null!;
        public int? courseId { get; set; }
        public int ratingValue { get; set; } // Giá trị đánh giá từ 0 đến 5
        public string? ratingReview { get; set; } // Đánh giá (nếu có)
        public bool replyExist { get; set; }
        public DateTime ratingRatingDate { get; set; } // Ngày đánh giá
        public List<string> ratingImageUrls { get; set; } = new(); // Danh sách URL ảnh liên quan
    }
}
