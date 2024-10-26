using System;
using System.Collections.Generic;

namespace study4_be.Models;

public partial class User
{
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    public string? UserEmail { get; set; }

    public string? UserPassword { get; set; }

    public string? UserDescription { get; set; }

    public string? UserImage { get; set; }

    public string? UserBanner { get; set; }

    public string? PhoneNumber { get; set; }

    public bool? Isverified { get; set; }

    public int? RoleId { get; set; }

    public double Blance { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<RatingReply> RatingReplies { get; set; } = new List<RatingReply>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();

    public virtual ICollection<UserDocument> UserDocuments { get; set; } = new List<UserDocument>();

    public virtual ICollection<UserSub> UserSubs { get; set; } = new List<UserSub>();

    public virtual ICollection<UsersExam> UsersExams { get; set; } = new List<UsersExam>();

    public virtual ICollection<Wallet> Wallets { get; set; } = new List<Wallet>();
}
