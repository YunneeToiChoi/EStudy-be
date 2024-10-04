using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class RatingImage
{
    public int ImageId { get; set; }

    public string ReferenceType { get; set; } = null!;

    public int ReferenceId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int? ReplyId { get; set; }

    public virtual Rating Reference { get; set; } = null!;

    public virtual RatingReply? Reply { get; set; }
}
