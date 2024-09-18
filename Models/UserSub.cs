using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class UserSub
{
    public string UsersubsId { get; set; } = null!;

    public string? UserId { get; set; }

    public int PlanId { get; set; }

    public DateOnly? UsersubsStartdate { get; set; }

    public DateOnly? UsersubsEnddate { get; set; }

    public double? UsersubsTotal { get; set; }

    public bool? State { get; set; }

    public virtual Subscriptionplan? Plan { get; set; }

    public virtual User? User { get; set; }
}
