﻿namespace study4_be.Services.Exam
{
    public class Part4Response
    {
        public int? questionId { get; set; }
        public int number { get; set; }
        public string questionImage { get; set; }
        public string questionText { get; set; }
        public string optionA { get; set; }
        public string optionB { get; set; }
        public string optionC { get; set; }
        public string optionD { get; set; }
        public string correctAnswear { get; set; }
    }
}
