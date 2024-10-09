namespace study4_be.Services.Exam
{
    public class ExamDetailResponse
    {
        public int userId { get; set; }      // ID của người dùng
        public int userExamId { get; set; }  // ID của bài thi mà người dùng đã tham gia
        public int examId { get; set; }      // ID của bài thi
        public string dateTime { get; set; } // Ngày giờ thi, định dạng chuỗi
        public string state { get; set; }    // Trạng thái bài thi (ví dụ: "Đã hoàn thành", "Chưa hoàn thành")
        public double? score { get; set; }   // Điểm của người dùng
        public string userTime { get; set; } // Thời gian làm bài thi của người dùng, chuyển đổi từ giây thành định dạng "HH:MM:SS"
    }
}
