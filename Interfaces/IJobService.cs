using ResumeAnalyzer.DTOs.Job;

namespace ResumeAnalyzer.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJobAsync(CreateJobDto dto);

        Task<List<JobResponseDto>> GetJobsAsync();

        Task<JobResponseDto?> GetJobByIdAsync(int id);

        Task<JobResponseDto?> UpdateJobAsync(int id, UpdateJobDto dto);

        Task<bool> DeleteJobAsync(int id);
    }
}
