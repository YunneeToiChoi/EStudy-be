using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Order
{
    public string OrderId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public int? CourseId { get; set; }

    public int? PlanId { get; set; }

    public int? DocumentId { get; set; }

    public DateTime OrderDate { get; set; }

    public double TotalAmount { get; set; }

    public string? Address { get; set; }

    public bool State { get; set; }

    public string? Email { get; set; }

    public string? Code { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? WalletId { get; set; }

    public string PaymentType { get; set; } = null!;

    public virtual Course? Course { get; set; }

    public virtual Document? Document { get; set; }

    public virtual Subscriptionplan? Plan { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Wallet? Wallet { get; set; }
}
