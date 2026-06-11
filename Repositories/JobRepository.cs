using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public class JobRepository : Repository<Job>, IJobRepository
    {
        public JobRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Job?> GetWithSkillsAsync(int id) =>
            await Query()
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.Creator)
                .FirstOrDefaultAsync(j => j.JobId == id);

        public async Task<IReadOnlyList<Job>> GetAllWithSkillsAsync() =>
            await Query()
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .Include(j => j.Creator)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
    }
}
