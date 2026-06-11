using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public class InterviewRepository : Repository<Interview>, IInterviewRepository
    {
        public InterviewRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Interview?> GetWithDetailsAsync(int id) =>
            await Query()
                .Include(i => i.Candidate)
                .Include(i => i.Job)
                .Include(i => i.Scheduler)
                .Include(i => i.Feedback)
                .FirstOrDefaultAsync(i => i.InterviewId == id);

        public async Task<IReadOnlyList<Interview>> GetAllWithDetailsAsync() =>
            await Query()
                .Include(i => i.Candidate)
                .Include(i => i.Job)
                .Include(i => i.Scheduler)
                .OrderByDescending(i => i.InterviewDate)
                .ToListAsync();

        public async Task<IReadOnlyList<Interview>> GetByCandidateIdAsync(int candidateId) =>
            await Query()
                .Include(i => i.Feedback)
                .Include(i => i.Job)
                .Where(i => i.CandidateId == candidateId)
                .OrderByDescending(i => i.InterviewDate)
                .ToListAsync();
    }
}
