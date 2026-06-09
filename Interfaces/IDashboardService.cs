using ResumeAnalyzer.DTOs.Dashboard;

namespace ResumeAnalyzer.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardDataAsync();

        Task<NotificationsResponseDto> GetNotificationsAsync();
    }
}
