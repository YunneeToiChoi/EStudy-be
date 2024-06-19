using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Department
    {
        public Department()
        {
            Staff = new HashSet<Staff>();
        }

        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public virtual ICollection<Staff> Staff { get; set; }
    }
}
