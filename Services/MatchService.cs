using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Match;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;
using System.Security.Claims;

namespace ResumeAnalyzer.Services
{
    public class MatchService : IMatchService
    {
        private readonly AppDbContext _context;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MatchService(
            AppDbContext context,
            ISettingsService settingsService,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<MatchResponseDto> MatchCandidateToJobAsync(
            int candidateId,
            int jobId)
        {
            var candidate = await _context.Candidates
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId)
                ?? throw new InvalidOperationException("Candidate not found");

            var jobSkills = await GetJobSkillNamesAsync(jobId);
            var candidateSkills = candidate.CandidateSkills
                .Select(cs => cs.Skill.SkillName)
                .ToList();

            var matchedSkills = MatchHelper.GetMatchedSkills(
                jobSkills,
                candidateSkills);

            var score = MatchHelper.CalculateScore(
                jobSkills,
                candidateSkills);

            var minMatchScore = await GetMinMatchScoreAsync();
            var status = MatchHelper.GetStatusFromScore(score, minMatchScore);

            var existing = await _context.CandidateJobMatches
                .FirstOrDefaultAsync(m =>
                    m.CandidateId == candidateId &&
                    m.JobId == jobId);

            if (existing != null)
            {
                existing.ATSScore = score;
                existing.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.CandidateJobMatches.Add(
                    new CandidateJobMatch
                    {
                        CandidateId = candidateId,
                        JobId = jobId,
                        ATSScore = score,
                        CreatedAt = DateTime.UtcNow
                    });
            }

            candidate.Status = status switch
            {
                "Shortlisted" => CandidateStatus.Shortlisted,
                "Review" => CandidateStatus.OnHold,
                _ => CandidateStatus.Parsed
            };
            candidate.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(candidate, jobId, matchedSkills, score, status);
        }

        public async Task<List<MatchResponseDto>> GetMatchesForJobAsync(int jobId)
        {
            var matches = await _context.CandidateJobMatches
                .Where(m => m.JobId == jobId)
                .OrderByDescending(m => m.ATSScore)
                .ToListAsync();

            var result = new List<MatchResponseDto>();

            foreach (var match in matches)
            {
                var candidate = await _context.Candidates
                    .Include(c => c.CandidateSkills)
                        .ThenInclude(cs => cs.Skill)
                    .FirstOrDefaultAsync(c =>
                        c.CandidateId == match.CandidateId);

                if (candidate == null)
                    continue;

                var jobSkills = await GetJobSkillNamesAsync(jobId);
                var candidateSkills = candidate.CandidateSkills
                    .Select(cs => cs.Skill.SkillName)
                    .ToList();

                var matchedSkills = MatchHelper.GetMatchedSkills(
                    jobSkills,
                    candidateSkills);

                var minMatchScore = await GetMinMatchScoreAsync();
                var status = MatchHelper.GetStatusFromScore(match.ATSScore, minMatchScore);

                result.Add(MapToDto(
                    candidate,
                    jobId,
                    matchedSkills,
                    match.ATSScore,
                    status));
            }

            return result;
        }

        public async Task<MatchResponseDto?> GetMatchAsync(
            int candidateId,
            int jobId)
        {
            var match = await _context.CandidateJobMatches
                .FirstOrDefaultAsync(m =>
                    m.CandidateId == candidateId &&
                    m.JobId == jobId);

            if (match == null)
                return null;

            var candidate = await _context.Candidates
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .FirstOrDefaultAsync(c => c.CandidateId == candidateId);

            if (candidate == null)
                return null;

            var jobSkills = await GetJobSkillNamesAsync(jobId);
            var candidateSkills = candidate.CandidateSkills
                .Select(cs => cs.Skill.SkillName)
                .ToList();

            var matchedSkills = MatchHelper.GetMatchedSkills(
                jobSkills,
                candidateSkills);

            var minMatchScore = await GetMinMatchScoreAsync();
            var status = MatchHelper.GetStatusFromScore(match.ATSScore, minMatchScore);

            return MapToDto(
                candidate,
                jobId,
                matchedSkills,
                match.ATSScore,
                status);
        }

        private async Task<int> GetMinMatchScoreAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;

            return await _settingsService.GetMinMatchScoreForUserAsync(email);
        }

        private async Task<List<string>> GetJobSkillNamesAsync(int jobId)
        {
            return await _context.JobSkills
                .Where(js => js.JobId == jobId)
                .Include(js => js.Skill)
                .Select(js => js.Skill.SkillName)
                .ToListAsync();
        }

        private static MatchResponseDto MapToDto(
            Candidate candidate,
            int jobId,
            List<string> matchedSkills,
            double score,
            string status)
        {
            return new MatchResponseDto
            {
                CandidateId = candidate.CandidateId,
                JobId = jobId,
                Name = candidate.FullName,
                Email = candidate.Email ?? "",
                Phone = candidate.Phone,
                Experience = candidate.ExperienceYears.HasValue
                    ? $"{candidate.ExperienceYears} years"
                    : "N/A",
                SkillsMatched = matchedSkills,
                MatchScore = score,
                Status = status
            };
        }
    }
}
