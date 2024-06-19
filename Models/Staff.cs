using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Staff
    {
        public Staff()
        {
            Reports = new HashSet<Report>();
        }

        public int StaffId { get; set; }
        public int? RoleId { get; set; }
        public string? StaffName { get; set; }
        public string? StaffType { get; set; }
        public int? DepartmentId { get; set; }

        public virtual Department? Department { get; set; }
        public virtual Role? Role { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
    }
}
