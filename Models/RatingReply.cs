using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class RatingReply
{
    public int ReplyId { get; set; }

    public int RatingId { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime ReplyDate { get; set; }

    public string ReplyContent { get; set; } = null!;

    public int? ParentReplyId { get; set; }

    public virtual ICollection<RatingReply> InverseParentReply { get; set; } = new List<RatingReply>();

    public virtual RatingReply? ParentReply { get; set; }

    public virtual Rating Rating { get; set; } = null!;

    public virtual ICollection<RatingImage> RatingImages { get; set; } = new List<RatingImage>();

    public virtual User User { get; set; } = null!;
}
