using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs.Settings;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var settings = await _settingsService.GetSettingsAsync(User);
            if (settings == null)
                return Unauthorized();

            return Ok(settings);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required.");

            var settings = await _settingsService.UpdateProfileAsync(User, dto);
            if (settings == null)
                return Unauthorized();

            return Ok(settings);
        }

        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences(UpdatePreferencesDto dto)
        {
            if (dto.MinMatchScore < 0 || dto.MinMatchScore > 100)
                return BadRequest("Minimum match score must be between 0 and 100.");

            var settings = await _settingsService.UpdatePreferencesAsync(User, dto);
            if (settings == null)
                return Unauthorized();

            return Ok(settings);
        }
    }
}
