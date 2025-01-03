﻿using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Subscriptionplan
{
    public int PlanId { get; set; }

    public string PlanName { get; set; } = null!;

    public double PlanPrice { get; set; }

    public int PlanDuration { get; set; }

    public string PlanDescription { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PlanCourse> PlanCourses { get; set; } = new List<PlanCourse>();

    public virtual ICollection<UserSub> UserSubs { get; set; } = new List<UserSub>();
}
