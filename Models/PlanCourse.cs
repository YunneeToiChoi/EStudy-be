using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class PlanCourse
{
    [Required]
    public int PlanId { get; set; }

    public int CourseId { get; set; }
}
