using ResumeAnalyzer.DTOs.InterviewFeedback;

namespace ResumeAnalyzer.Interfaces
{
    public interface IInterviewFeedbackService
    {
        Task<InterviewFeedbackResponseDto> CreateFeedbackAsync(
            CreateInterviewFeedbackDto dto,
            int managerId);
        Task<List<InterviewFeedbackResponseDto>> GetFeedbackByCandidateIdAsync(int candidateId);
    }
}
