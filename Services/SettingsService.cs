using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Settings;
using ResumeAnalyzer.Interfaces;
using System.Security.Claims;

namespace ResumeAnalyzer.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly AppDbContext _context;

        public SettingsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SettingsResponseDto?> GetSettingsAsync(ClaimsPrincipal user)
        {
            var dbUser = await GetUserAsync(user);
            return dbUser == null ? null : MapToDto(dbUser);
        }

        public async Task<SettingsResponseDto?> UpdateProfileAsync(
            ClaimsPrincipal user,
            UpdateProfileDto dto)
        {
            var dbUser = await GetUserAsync(user);
            if (dbUser == null)
                return null;

            dbUser.Name = dto.Name.Trim();
            dbUser.Organization = dto.Organization.Trim();

            await _context.SaveChangesAsync();

            return MapToDto(dbUser);
        }

        public async Task<SettingsResponseDto?> UpdatePreferencesAsync(
            ClaimsPrincipal user,
            UpdatePreferencesDto dto)
        {
            var dbUser = await GetUserAsync(user);
            if (dbUser == null)
                return null;

            dbUser.EmailNewResumes = dto.EmailNewResumes;
            dbUser.NotifyMatchComplete = dto.NotifyMatchComplete;
            dbUser.WeeklyAnalyticsReport = dto.WeeklyAnalyticsReport;
            dbUser.MinMatchScore = Math.Clamp(dto.MinMatchScore, 0, 100);

            await _context.SaveChangesAsync();

            return MapToDto(dbUser);
        }

        public async Task<int> GetMinMatchScoreForUserAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return 60;

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            return user?.MinMatchScore ?? 60;
        }

        private async Task<Models.User?> GetUserAsync(ClaimsPrincipal principal)
        {
            var email = principal.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        private static SettingsResponseDto MapToDto(Models.User user) =>
            new()
            {
                Name = user.Name,
                Email = user.Email,
                Organization = user.Organization,
                Role = user.Role,
                EmailNewResumes = user.EmailNewResumes,
                NotifyMatchComplete = user.NotifyMatchComplete,
                WeeklyAnalyticsReport = user.WeeklyAnalyticsReport,
                MinMatchScore = user.MinMatchScore
            };
    }
}
