using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class UsersExam
    {
        public UsersExam()
        {
            UserAnswers = new HashSet<UserAnswer>();
        }

        public string UserId { get; set; } = null!;
        public string UserExamId { get; set; } = null!;
        public string ExamId { get; set; } = null!;
        public DateTime? DateTime { get; set; }
        public bool? State { get; set; }
        public int? Score { get; set; }

        public virtual Exam Exam { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<UserAnswer> UserAnswers { get; set; }
    }
}
