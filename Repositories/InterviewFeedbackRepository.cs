using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public class InterviewFeedbackRepository
        : Repository<InterviewFeedback>, IInterviewFeedbackRepository
    {
        public InterviewFeedbackRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<InterviewFeedback>> GetByCandidateIdAsync(int candidateId) =>
            await Query()
                .Include(f => f.Manager)
                .Include(f => f.Interview)
                .Where(f => f.CandidateId == candidateId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

        public async Task<InterviewFeedback?> GetByInterviewIdAsync(int interviewId) =>
            await Query()
                .Include(f => f.Manager)
                .FirstOrDefaultAsync(f => f.InterviewId == interviewId);
    }
}
