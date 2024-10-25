using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Wallet
{
    public string Id { get; set; } = null!;


    public string Userid { get; set; } = null!;

    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;

    public string CardNumber { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User User { get; set; } = null!;
}
