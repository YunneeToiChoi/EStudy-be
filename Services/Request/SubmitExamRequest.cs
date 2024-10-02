using System.ComponentModel.DataAnnotations;
using System.Drawing.Text;

namespace study4_be.Services.Request
{
    public class SubmitExamRequest
    {
        [Required]
        public required string examId { get;set; }
        [Required]
        public required string userId { get;set; }
        public int? score { get;set; }
        [Required]
        public required int userTime { get; set; }
        public List<AnswerDto> answer { get; set; }

    }
    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
        public bool State { get; set; }
    }
}
