﻿namespace study4_be.Services.Rating
{
    public class RatingDocumentResponse
    {
        public int ratingId { get; set; }
        public string userId { get; set; } = null!;
        public string userImage { get; set; } = null!;
        public string userName { get; set; } = null!;
        public int? documentId { get; set; }
        public int ratingValue { get; set; } // Giá trị đánh giá từ 0 đến 5
        public bool replyExist { get; set; }
        public int childAmount { get; set; }
        public string? ratingReview { get; set; } // Đánh giá (nếu có)
        public DateTime ratingDate { get; set; } // Ngày đánh giá
        public List<string> ratingImageUrls { get; set; } = new(); // Danh sách URL ảnh liên quan
    }
}
