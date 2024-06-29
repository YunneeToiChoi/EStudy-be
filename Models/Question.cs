using System;
using System.Collections.Generic;

namespace study4_be.Models
{
    public partial class Question
    {
        public Question()
        {
            Exams = new HashSet<Exam>();
        }

        public int QuestionId { get; set; }
        public int? LessonId { get; set; }
        public string? QuestionText { get; set; }
        public string? QuestionTextMean { get; set; }
        public string? QuestionParagraph { get; set; }
        public string? QuestionParagraphMean { get; set; }
        public string? QuestionTranslate { get; set; }
        public string? QuestionAudio { get; set; }
        public string? QuestionImage { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? OptionMeanA { get; set; }
        public string? OptionMeanB { get; set; }
        public string? OptionMeanC { get; set; }
        public string? OptionMeanD { get; set; }
        public string? QuestionType { get; set; }

        public virtual Lesson? Lesson { get; set; }

        public virtual ICollection<Exam> Exams { get; set; }
    }
}
