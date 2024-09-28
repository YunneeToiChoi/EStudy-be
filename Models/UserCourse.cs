﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class UserCourse
{
    [Required]
    public string UserId { get; set; } = null!;

    public int CourseId { get; set; }

    public DateTime? Date { get; set; }

    public int? Process { get; set; }

    public bool? State { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual User User { get; set; } = null!;
}
