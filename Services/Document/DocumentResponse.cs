namespace study4_be.Services.Document
{
    public class DocumentResponse
    {
        public int documentId { get; set; }
        public int? downloadCount { get; set; }
        public string title { get; set; }
        public bool? isPublic { get; set; }
        public string fileType { get; set; }
        public string thumbnailUrl { get; set; }
        public string userId { get; set; }
        public string userName { get; set; }
        public string userImage { get; set; }
        public string documentDescription { get; set; }
    }
}
