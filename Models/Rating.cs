using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Rating
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public DateTime RatingDate { get; set; }

    public short RatingValue { get; set; }

    public string? Review { get; set; }

    public int? CourseId { get; set; }

    public int? DocumentId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Document? Document { get; set; }

    public virtual ICollection<RatingImage> RatingImages { get; set; } = new List<RatingImage>();

    public virtual ICollection<RatingReply> RatingReplies { get; set; } = new List<RatingReply>();

    public virtual User User { get; set; } = null!;
}
