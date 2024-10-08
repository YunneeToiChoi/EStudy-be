﻿using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class UserSub
{
    public string UserId { get; set; } = null!;

    public int PlanId { get; set; }

    public DateTime UsersubsStartdate { get; set; }

    public DateTime UsersubsEnddate { get; set; }

    public bool State { get; set; }

    public virtual Subscriptionplan Plan { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
