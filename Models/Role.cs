using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Models;

public partial class Role
{
    [JsonRequired]
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
