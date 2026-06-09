using ResumeAnalyzer.DTOs.Resume;

namespace ResumeAnalyzer.Interfaces
{
    public interface IResumeService
    {
        Task<ResumeResponseDto> UploadResumeAsync(ResumeUploadDto dto);

        Task<List<ResumeResponseDto>> GetResumesByJobAsync(int jobId);

        Task<bool> DeleteResumeAsync(int resumeId);
    }
}
