﻿using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Rating
    {
        public int RatingId { get; set; }
        public string? UserId { get; set; }
        public int? CourseId { get; set; }
        public DateTime? RatingDate { get; set; }
        public short? RatingValue { get; set; }
        public string? Review { get; set; }

        public virtual Course? Course { get; set; }
        public virtual User? User { get; set; }
    }
}
