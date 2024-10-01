
public class DocumentDto
{
    public int DocumentId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string FileUrl { get; set; }
    public DateTime? UploadDate { get; set; }
    public string? FileType { get; set; }
    public int? DownloadCount { get; set; }
    public string? ThumbnailUrl { get; set; }
}