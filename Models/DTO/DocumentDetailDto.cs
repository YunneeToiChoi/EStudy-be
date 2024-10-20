namespace study4_be.Models.DTO
{
    public class DocumentDetailDto
    {
        public int documentId { get; set; }
        public string title { get; set; }
        public string? documentDescription { get; set; }
        public string fileUrl { get; set; }
        public string previewUrl { get; set; }
        public DateTime? uploadDate { get; set; }
        public string? fileType { get; set; }
        public bool? documentPublic { get; set; }
        public int? downloadCount { get; set; }
        public double? price { get; set; }
        public int userDocumentCount { get; set; } // Số lượng tài liệu người dùng đã đăng
        public int userDownloadCount { get; set; } // Số lượng người dùng đã tải tài liệu
        public UserDto user { get; set; }
    }
}
