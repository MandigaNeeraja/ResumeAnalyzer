using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public interface IJobRepository : IRepository<Job>
    {
        Task<Job?> GetWithSkillsAsync(int id);
        Task<IReadOnlyList<Job>> GetAllWithSkillsAsync();
    }
}
