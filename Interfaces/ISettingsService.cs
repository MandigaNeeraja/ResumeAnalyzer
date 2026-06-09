using ResumeAnalyzer.DTOs.Settings;
using System.Security.Claims;

namespace ResumeAnalyzer.Interfaces
{
    public interface ISettingsService
    {
        Task<SettingsResponseDto?> GetSettingsAsync(ClaimsPrincipal user);

        Task<SettingsResponseDto?> UpdateProfileAsync(ClaimsPrincipal user, UpdateProfileDto dto);

        Task<SettingsResponseDto?> UpdatePreferencesAsync(ClaimsPrincipal user, UpdatePreferencesDto dto);

        Task<int> GetMinMatchScoreForUserAsync(string? email);
    }
}
