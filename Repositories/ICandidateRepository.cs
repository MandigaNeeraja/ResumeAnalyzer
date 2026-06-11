using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public interface ICandidateRepository : IRepository<Candidate>
    {
        Task<Candidate?> GetWithDetailsAsync(int id);
        Task<IReadOnlyList<Candidate>> GetByStatusAsync(CandidateStatus status);
        Task<IReadOnlyList<Candidate>> GetByStatusesAsync(IEnumerable<CandidateStatus> statuses);
        Task<IReadOnlyList<Candidate>> GetAllWithDetailsAsync();
    }
}
