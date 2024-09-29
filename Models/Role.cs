using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Role
{
    [Required]
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
