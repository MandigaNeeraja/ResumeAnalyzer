using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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
            return Ok(await _dashboardService.GetDashboardDataAsync());
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            return Ok(await _dashboardService.GetNotificationsAsync());
        }
    }
}
