using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class User
    {
        public User()
        {
            Orders = new HashSet<Order>();
            Ratings = new HashSet<Rating>();
            UserCourses = new HashSet<UserCourse>();
        }

        public string UserId { get; set; } = null!;
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPassword { get; set; }
        public string? UserDescription { get; set; }
        public string? UserImage { get; set; }
        public string? UserBanner { get; set; }
        public string? PhoneNumber { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Rating> Ratings { get; set; }
        public virtual ICollection<UserCourse> UserCourses { get; set; }
    }
}
