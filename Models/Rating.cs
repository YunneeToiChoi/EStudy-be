using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Rating
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public int EntityId { get; set; }

    public DateTime RatingDate { get; set; }

    public short RatingValue { get; set; }

    public string? Review { get; set; }

    public virtual Course Entity { get; set; } = null!;

    public virtual Document EntityNavigation { get; set; } = null!;

    public virtual ICollection<RatingImage> RatingImages { get; set; } = new List<RatingImage>();

    public virtual ICollection<RatingReply> RatingReplies { get; set; } = new List<RatingReply>();

    public virtual User User { get; set; } = null!;
}
