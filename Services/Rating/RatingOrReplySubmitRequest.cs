﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Services.Rating
{
    public class RatingOrReplySubmitRequest
    {
        [Required]
        public string userId { get; set; }
        [Required]
        public string ratingEntityType { get; set; } // "Document" hoặc "Course"
        public int? documentId { get; set; }
        public int? courseId { get; set; }
        [JsonRequired]
        public short ratingValue { get; set; } // Giá trị từ 1-5
        [Required]
        public string ratingReview { get; set; } // Nhận xét (tùy chọn)
        [JsonRequired]
        public bool isRating { get; set; }
        public int? parentReply { get; set; } // case reply
        public int rootId { get; set; }
    }
}
