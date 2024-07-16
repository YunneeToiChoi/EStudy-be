using study4_be.Models;

namespace study4_be.Services.Response
{
    public class UserExamResponse
    {
        public string userId { get; set; }
        public string userExamId { get; set; }
        public string userTime { get; set; }
        public string examId { get; set; }
        public string dateTime { get; set; }
        public bool? state { get; set; }
        public int? score { get; set; }
    }

}
