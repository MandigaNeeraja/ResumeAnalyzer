using ResumeAnalyzer.DTOs.Candidate;
using ResumeAnalyzer.DTOs.Match;

namespace ResumeAnalyzer.Interfaces
{
    public interface ICandidateService
    {
        Task<List<CandidateResponseDto>> GetCandidatesAsync();

        Task<CandidateResponseDto?> GetCandidateByIdAsync(int id);

        Task<MatchResponseDto?> GetCandidateMatchForJobAsync(
            int candidateId,
            int jobId);
    }
}
