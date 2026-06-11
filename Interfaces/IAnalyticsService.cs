using ResumeAnalyzer.DTOs.Analytics;

namespace ResumeAnalyzer.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsResponseDto> GetAnalyticsAsync(string? role = null);
    }
}
