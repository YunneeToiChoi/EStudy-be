using study4_be.Models;

public class UserDocumentResponse
{
    public int documentId { get; set; }

    public string userId { get; set; } = null!;

    public string title { get; set; } = null!;

    public string? documentDescription { get; set; }

    public string fileUrl { get; set; } = null!;

    public DateTime? uploadDate { get; set; }

    public string? fileType { get; set; }

    public bool? isPublic { get; set; }

    public int? downloadCount { get; set; }

    public int? categoryId { get; set; }
    public string? categoryName { get;set; }

    public int? courseId { get; set; }
    public string? courseName { get; set; }

    public double? price { get; set; }

    public double? documentSize { get; set; }
    public string? thumbnailUrl {  get; set; } 

}