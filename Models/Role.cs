using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
