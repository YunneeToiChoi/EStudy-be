using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Document
{
    public int DocumentId { get; set; }

    public string UserId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string FileUrl { get; set; } = null!;

    public DateTime? UploadDate { get; set; }

    public string? FileType { get; set; }

    public bool? IsPublic { get; set; }

    public int? DownloadCount { get; set; }

    public int? CategoryId { get; set; }

    public int? CourseId { get; set; }

    public double? Price { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Course? Course { get; set; }

    public virtual User User { get; set; } = null!;
}
