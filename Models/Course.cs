using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string? CourseName { get; set; }

    public string? CourseDescription { get; set; }

    public string? CourseImage { get; set; }

    public string? CourseTag { get; set; }

    public double? CoursePrice { get; set; }

    public int? CourseSale { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PlanCourse> PlanCourses { get; set; } = new List<PlanCourse>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();

    public virtual ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
}
