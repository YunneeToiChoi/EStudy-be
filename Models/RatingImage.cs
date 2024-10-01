using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class RatingImage
{
    public int ImageId { get; set; }

    public int RatingId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Rating Rating { get; set; } = null!;
}
