using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Interfaces;
using System.Security.Claims;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return role switch
            {
                Roles.Admin => Ok(await _dashboardService.GetAdminDashboardAsync()),
                Roles.HR => Ok(await _dashboardService.GetHRDashboardAsync()),
                Roles.Manager => Ok(await _dashboardService.GetManagerDashboardAsync()),
                _ => Ok(await _dashboardService.GetDashboardDataAsync())
            };
        }

        [HttpGet("admin")]
        [Authorize(Roles = Roles.AdminOnly)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            return Ok(await _dashboardService.GetAdminDashboardAsync());
        }

        [HttpGet("hr")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> GetHRDashboard()
        {
            return Ok(await _dashboardService.GetHRDashboardAsync());
        }

        [HttpGet("manager")]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> GetManagerDashboard()
        {
            return Ok(await _dashboardService.GetManagerDashboardAsync());
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(await _dashboardService.GetNotificationsAsync(role));
        }
    }
}
