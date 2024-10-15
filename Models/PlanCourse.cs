using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class PlanCourse
{
    public int PlanId { get; set; }

    public int CourseId { get; set; }

    public virtual Subscriptionplan Plan { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
}
