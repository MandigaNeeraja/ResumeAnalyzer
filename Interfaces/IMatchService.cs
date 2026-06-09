using ResumeAnalyzer.DTOs.Match;

namespace ResumeAnalyzer.Interfaces
{
    public interface IMatchService
    {
        Task<MatchResponseDto> MatchCandidateToJobAsync(
            int candidateId,
            int jobId);

        Task<List<MatchResponseDto>> GetMatchesForJobAsync(int jobId);

        Task<MatchResponseDto?> GetMatchAsync(
            int candidateId,
            int jobId);
    }
}
