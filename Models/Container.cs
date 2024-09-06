using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace study4_be.Models
{
    public partial class Container
    {
        public Container()
        {
            Lessons = new HashSet<Lesson>();
        }

        public int ContainerId { get; set; }
        [Required(ErrorMessage = "Container Title is required")]
        public string? ContainerTitle { get; set; }

        [Required(ErrorMessage = "Unit selection is required")]
        public int? UnitId { get; set; }

        public virtual Unit? Unit { get; set; }
        public virtual ICollection<Lesson> Lessons { get; set; }
    }
}
