namespace study4_be.Services.Request.Document
{
    public class UploadDetailRequest
    {
        public IEnumerable<int> idDocuments {  get; set; }
        public string userId { get; set; }
        public int? categoryId { get; set; } 
        public int? courseId { get; set; }
        public bool state { get; set; }
        public string? description { get; set; }
        public double? price { get; set; }
        public string title { get;set; }
    }
}
