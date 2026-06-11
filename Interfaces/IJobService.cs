using ResumeAnalyzer.DTOs.Job;

namespace ResumeAnalyzer.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJobAsync(CreateJobDto dto);
        Task<List<JobResponseDto>> GetJobsAsync(JobFilterDto? filter = null);
        Task<JobResponseDto?> GetJobByIdAsync(int id);
        Task<JobResponseDto?> UpdateJobAsync(int id, UpdateJobDto dto);
        Task<bool> DeleteJobAsync(int id);
        Task<JobResponseDto?> CloseJobAsync(int id);
        Task<JobResponseDto?> ReopenJobAsync(int id);
        Task<JobResponseDto?> HoldJobAsync(int id);
        Task<List<string>> GetDepartmentsAsync();
        Task<JobCandidateSummaryDto> GetCandidateSummaryAsync(int jobId);
        Task<List<JobActivityLogDto>> GetActivityLogsAsync(int jobId);
    }
}
