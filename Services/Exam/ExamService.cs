using Microsoft.EntityFrameworkCore;
using study4_be.Interface.Exam;
using study4_be.Models;

namespace study4_be.Services.Exam
{
    public class ExamService : IExamService
    {
        private readonly Study4Context _context;

        public ExamService(Study4Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExamPreviewDetails>> GetAllExamsAsync()
        {
            try
            {
                var exams = await _context.Exams
                    .AsNoTracking()
                    .ToListAsync();
                if (exams == null || exams.Count == 0)
                {
                    throw new Exception("No courses found.");
                }

                return exams.Select(exam => new ExamPreviewDetails
                {
                    ExamId = exam.ExamId,
                    ExamName = exam.ExamName,
                    ExamImage = exam.ExamImage,
                    TotalUsersTest = TotalUsersTest(exam.ExamId),
                    TotalComments = TotalExamComments(exam.ExamId),
                    TotalMinutes = TotalExamMinutes(exam.ExamId),
                }).ToList();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<IEnumerable<UsersExamResponse>> GetUserExamsAsync(string userId)
        {
            try
            {
                return await _context.UsersExams
                    .AsNoTracking()
                    .Where(ux => ux.UserId == userId)
                    .Join(_context.Exams.AsNoTracking(), // khong theo doi neu ko thao tac -> toi uu performance
                        ux => ux.ExamId,
                        e => e.ExamId,
                        (ux, e) => new UsersExamResponse
                        {
                            ExamId = e.ExamId,
                            ExamName = e.ExamName,
                            ExamImage = e.ExamImage,
                            UserExamId = ux.UserExamId,
                            DateTime = ux.DateTime,
                            State = ux.State,
                            Score = ux.Score,
                            UserTime = ux.UserTime,
                        })
                    .ToListAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private int TotalUsersTest(string examId)
        {
            return _context.UsersExams
                .Where(ue => ue.ExamId == examId)
                .AsNoTracking()
                .Count();
        }

        private int TotalExamMinutes(string examId)
        {
            return 120;
        }

        private int TotalExamComments(string examId)
        {
            return 100;
        }
    }
}