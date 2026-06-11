using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public class CandidateRepository : Repository<Candidate>, ICandidateRepository
    {
        public CandidateRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Candidate?> GetWithDetailsAsync(int id) =>
            await Query()
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Include(c => c.Job)
                .Include(c => c.Resume)
                .FirstOrDefaultAsync(c => c.CandidateId == id);

        public async Task<IReadOnlyList<Candidate>> GetByStatusAsync(CandidateStatus status) =>
            await Query()
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Include(c => c.Job)
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<IReadOnlyList<Candidate>> GetByStatusesAsync(
            IEnumerable<CandidateStatus> statuses) =>
            await Query()
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Include(c => c.Job)
                .Where(c => statuses.Contains(c.Status))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<IReadOnlyList<Candidate>> GetAllWithDetailsAsync() =>
            await Query()
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Include(c => c.Job)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
    }
}
