namespace study4_be.Services.Rating
{
    public class RatingUserResponse
    {
        public int ratingId { get; set; }
        public string userId { get; set; } = null!;
        public string userImage { get; set; } = null!;
        public int ratingEntityId { get; set; } // ID của tài liệu hoặc khóa học mà người dùng đã đánh giá
        public string ratingEntityType { get; set; } = null!; // Loại đối tượng (Document, Course)
        public int ratingValue { get; set; } // Giá trị đánh giá từ 0 đến 5
        public string? ratingReview { get; set; } // Đánh giá (nếu có)
        public DateTime ratingDate { get; set; } // Ngày đánh giá
        public List<string> ratingImageUrls { get; set; } = new(); // Danh sách URL ảnh liên quan
    }
}
