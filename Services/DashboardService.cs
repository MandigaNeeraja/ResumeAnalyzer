using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Dashboard;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponseDto> GetDashboardDataAsync()
        {
            var totalResumes = await _context.Resumes.CountAsync();
            var shortlisted = await _context.Candidates
                .CountAsync(c => c.Status == CandidateStatus.Shortlisted);
            var openJobs = await _context.Jobs.CountAsync();
            var matchRequests = await _context.CandidateJobMatches.CountAsync();

            var topCandidates = await _context.CandidateJobMatches
                .OrderByDescending(m => m.ATSScore)
                .Take(5)
                .Join(
                    _context.Candidates,
                    m => m.CandidateId,
                    c => c.CandidateId,
                    (m, c) => new TopCandidateDto
                    {
                        Name = c.FullName,
                        Match = m.ATSScore
                    })
                .ToListAsync();

            var recentActivity = await BuildRecentActivityAsync(5);

            return new DashboardResponseDto
            {
                Stats = new DashboardStatsDto
                {
                    TotalResumes = totalResumes,
                    Shortlisted = shortlisted,
                    OpenJobs = openJobs,
                    MatchRequests = matchRequests
                },
                TopCandidates = topCandidates,
                RecentActivity = recentActivity
            };
        }

        public async Task<NotificationsResponseDto> GetNotificationsAsync()
        {
            var recentActivity = await BuildRecentActivityAsync(10);

            return new NotificationsResponseDto
            {
                Count = recentActivity.Count,
                Items = recentActivity.Select(a => new NotificationDto
                {
                    Message = a.Action,
                    Detail = a.Detail,
                    OccurredAt = a.OccurredAt,
                    Type = a.Type
                }).ToList()
            };
        }

        private async Task<List<RecentActivityDto>> BuildRecentActivityAsync(int take)
        {
            var cutoff = DateTime.UtcNow.AddDays(-7);

            var recentResumes = await _context.Resumes
                .Where(r => r.ParsedAt >= cutoff)
                .OrderByDescending(r => r.ParsedAt)
                .Take(5)
                .Join(
                    _context.Candidates,
                    r => r.CandidateId,
                    c => c.CandidateId,
                    (r, c) => new RecentActivityDto
                    {
                        Action = "Resume parsed",
                        Detail = $"{c.FullName} - {r.FileName}",
                        OccurredAt = AsUtc(r.ParsedAt),
                        Type = "parse"
                    })
                .ToListAsync();

            var recentJobs = await _context.Jobs
                .Where(j => j.CreatedAt >= cutoff)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(j => new RecentActivityDto
                {
                    Action = "New job added",
                    Detail = j.Title,
                    OccurredAt = AsUtc(j.CreatedAt),
                    Type = "job"
                })
                .ToListAsync();

            var recentMatches = await _context.CandidateJobMatches
                .Where(m => m.CreatedAt >= cutoff)
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .Join(
                    _context.Candidates,
                    m => m.CandidateId,
                    c => c.CandidateId,
                    (m, c) => new { m, c })
                .Join(
                    _context.Jobs,
                    mc => mc.m.JobId,
                    j => j.JobId,
                    (mc, j) => new RecentActivityDto
                    {
                        Action = "Match completed",
                        Detail = $"{mc.c.FullName} matched for {j.Title}",
                        OccurredAt = AsUtc(mc.m.CreatedAt),
                        Type = "match"
                    })
                .ToListAsync();

            return recentResumes
                .Concat(recentJobs)
                .Concat(recentMatches)
                .OrderByDescending(a => a.OccurredAt)
                .Take(take)
                .ToList();
        }

        private static DateTime AsUtc(DateTime dateTime) =>
            dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
