using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Unit
    {
        public Unit()
        {
            Containers = new HashSet<Container>();
        }

        public int UnitId { get; set; }
        public int? CourseId { get; set; }
        public string? UnitTittle { get; set; }
        public int? Process { get; set; }

        public virtual Course? Course { get; set; }
        public virtual ICollection<Container> Containers { get; set; }
    }
}
