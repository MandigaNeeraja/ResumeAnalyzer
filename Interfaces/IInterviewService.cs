using ResumeAnalyzer.DTOs.Interview;

namespace ResumeAnalyzer.Interfaces
{
    public interface IInterviewService
    {
        Task<InterviewResponseDto> CreateInterviewAsync(CreateInterviewDto dto, int scheduledByUserId);
        Task<List<InterviewResponseDto>> GetInterviewsAsync();
        Task<InterviewResponseDto?> GetInterviewByIdAsync(int id);
        Task<InterviewResponseDto?> UpdateInterviewAsync(int id, UpdateInterviewDto dto);
        Task<InterviewResponseDto?> CompleteInterviewAsync(int id);
    }
}
