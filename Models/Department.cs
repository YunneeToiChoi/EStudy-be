using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models;

public partial class Department
{
    [Required]

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
