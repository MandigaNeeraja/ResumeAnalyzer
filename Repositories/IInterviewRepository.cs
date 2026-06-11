using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public interface IInterviewRepository : IRepository<Interview>
    {
        Task<Interview?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<Interview>> GetAllWithDetailsAsync();
        Task<IReadOnlyList<Interview>> GetByCandidateIdAsync(int candidateId);
    }
}
