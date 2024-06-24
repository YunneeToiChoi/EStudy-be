using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Course
    {
        public Course()
        {
            Orders = new HashSet<Order>();
            Units = new HashSet<Unit>();
            UserCourses = new HashSet<UserCourse>();
        }

        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseDescription { get; set; }
        public string? CourseImage { get; set; }
        public string? CourseTag { get; set; }
        public double? CoursePrice { get; set; }
        public int? CourseSale { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Unit> Units { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
    }
}
