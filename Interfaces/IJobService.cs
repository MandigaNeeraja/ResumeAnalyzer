using ResumeAnalyzer.DTOs.Job;

namespace ResumeAnalyzer.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJobAsync(CreateJobDto dto);

        Task<List<JobResponseDto>> GetJobsAsync();

        Task<JobResponseDto?> GetJobByIdAsync(int id);
    }
}