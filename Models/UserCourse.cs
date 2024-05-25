﻿using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class UserCourse
    {
        public string UserId { get; set; } = null!;
        public int CourseId { get; set; }
        public DateTime? Date { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
