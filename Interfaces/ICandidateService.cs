using ResumeAnalyzer.DTOs.Candidate;
using ResumeAnalyzer.DTOs.Match;

namespace ResumeAnalyzer.Interfaces
{
    public interface ICandidateService
    {
        Task<List<CandidateResponseDto>> GetCandidatesAsync();
        Task<List<CandidateResponseDto>> GetCandidatesForRoleAsync(string role);
        Task<CandidateResponseDto?> GetCandidateByIdAsync(int id);
        Task<CandidateResponseDto?> GetCandidateByIdForRoleAsync(int id, string role);
        Task<MatchResponseDto?> GetCandidateMatchForJobAsync(int candidateId, int jobId);
        Task<List<CandidateResponseDto>> GetHrScreeningCandidatesAsync();
        Task<List<CandidateResponseDto>> GetManagerReviewCandidatesAsync();
        Task<CandidateResponseDto?> SendToManagerAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> ShortlistCandidateAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> RejectCandidateAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> HoldCandidateAsync(int id, CandidateWorkflowDto dto, bool isManager);
        Task<CandidateResponseDto?> ResumeToScreeningAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> ResumeToManagerReviewAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> ApproveInterviewAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> TechnicalSelectAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> TechnicalRejectAsync(int id, CandidateWorkflowDto dto);
        Task<CandidateResponseDto?> HireAsync(int id, CandidateWorkflowDto dto);
    }
}
