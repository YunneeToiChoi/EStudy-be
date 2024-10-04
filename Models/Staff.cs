using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace study4_be.Models;

public partial class Staff
{
    [JsonRequired]
    public int StaffId { get; set; }

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }

    public string? StaffName { get; set; }

    public string? StaffType { get; set; }

    public string? StaffEmail { get; set; }

    public string StaffCmnd { get; set; } = null!;

    public virtual Department? Department { get; set; }

    public virtual Role? Role { get; set; }
}
