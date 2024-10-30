using Microsoft.AspNetCore.Mvc;
using study4_be.Services.Exam;

namespace study4_be.Interface;

public interface IWritingService
{
    Task<WritingScore> ScoringWritingAsync(int maxScore, string content, int modelIndex);
}