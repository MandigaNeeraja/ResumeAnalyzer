using ResumeAnalyzer.DTOs.Dashboard;

namespace ResumeAnalyzer.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardDataAsync();
        Task<NotificationsResponseDto> GetNotificationsAsync(string? role);
        Task<AdminDashboardDto> GetAdminDashboardAsync();
        Task<HRDashboardDto> GetHRDashboardAsync();
        Task<ManagerDashboardDto> GetManagerDashboardAsync();
    }
}
