using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Lesson
    {
        public Lesson()
        {
            Questions = new HashSet<Question>();
            Videos = new HashSet<Video>();
            Vocabularies = new HashSet<Vocabulary>();
        }

        public int LessonId { get; set; }
        public string? LessonType { get; set; }
        public string? LessonTitle { get; set; }
        public int? ContainerId { get; set; }
        public string? TagId { get; set; }

        public virtual Container? Container { get; set; }
        public virtual Tag? Tag { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Video> Videos { get; set; }
        public virtual ICollection<Vocabulary> Vocabularies { get; set; }
    }
}
