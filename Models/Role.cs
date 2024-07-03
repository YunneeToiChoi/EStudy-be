using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Role
    {
        public Role()
        {
            Staff = new HashSet<Staff>();
            Users = new HashSet<User>();
        }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        public virtual ICollection<Staff> Staff { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
