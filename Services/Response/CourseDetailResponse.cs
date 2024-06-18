namespace study4_be.Services.Response
{
    public class CourseDetailResponse
    {
        public int? courseId { get; set; }
        public string? courseName { get; set; } 
        public string? courseDescription { get; set; }
        public string? courseTag { get; set; }  
        public double? coursePrice { get; set;}
        public string? courseSale { get; set;}
        public string? finalPrice { get; set; } 
    }
}
