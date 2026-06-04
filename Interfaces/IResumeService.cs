using ResumeAnalyzer.DTOs.Resume;

namespace ResumeAnalyzer.Interfaces
{
    public interface IResumeService
    {
        Task<ResumeResponseDto> UploadResumeAsync(
            ResumeUploadDto dto);
    }
}