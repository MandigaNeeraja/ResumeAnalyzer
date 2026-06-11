using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Analytics;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsResponseDto> GetAnalyticsAsync(string? role = null)
        {
            var isManager = role == Roles.Manager;
            var candidatesQuery = _context.Candidates.AsQueryable();

            if (isManager)
            {
                candidatesQuery = candidatesQuery.Where(c =>
                    CandidateVisibilityHelper.ManagerVisibleStatuses.Contains(c.Status));
            }

            var totalCandidates = await candidatesQuery.CountAsync();
            var totalResumes = await _context.Resumes.CountAsync();
            var parsedResumes = await _context.Resumes.CountAsync(r => r.ParseStatus == "Parsed");
            var openJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Open);

            var matchesQuery = _context.CandidateJobMatches.AsQueryable();
            if (isManager)
            {
                matchesQuery = matchesQuery.Where(m =>
                    _context.Candidates.Any(c =>
                        c.CandidateId == m.CandidateId &&
                        CandidateVisibilityHelper.ManagerVisibleStatuses.Contains(c.Status)));
            }

            var matchedCandidates = await matchesQuery
                .Select(m => m.CandidateId)
                .Distinct()
                .CountAsync();

            var shortlisted = await candidatesQuery
                .CountAsync(c => c.Status == CandidateStatus.Shortlisted ||
                                 c.Status == CandidateStatus.TechnicalSelected);

            var onReview = await candidatesQuery.CountAsync(c =>
                c.Status == CandidateStatus.HRScreening ||
                c.Status == CandidateStatus.SentToManager ||
                c.Status == CandidateStatus.OnHold);

            var inProgress = await candidatesQuery.CountAsync(c =>
                c.Status == CandidateStatus.Applied ||
                c.Status == CandidateStatus.InterviewScheduled ||
                c.Status == CandidateStatus.InterviewCompleted);

            var hired = await candidatesQuery.CountAsync(c => c.Status == CandidateStatus.Hired);

            var totalMatches = await matchesQuery.CountAsync();
            var averageScore = await matchesQuery
                .Select(m => (double?)m.ATSScore)
                .AverageAsync() ?? 0;

            var statusCounts = await candidatesQuery
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusDistribution = statusCounts
                .Select(s => new StatusDistributionDto
                {
                    Name = FormatStatus(s.Status),
                    Value = s.Count
                })
                .Where(s => s.Value > 0)
                .OrderByDescending(s => s.Value)
                .ToList();

            var groupedStatusDistribution = new List<StatusDistributionDto>
            {
                new() { Name = "In Progress", Value = inProgress },
                new() { Name = "Shortlisted", Value = shortlisted + hired },
                new() { Name = "Under Review", Value = onReview }
            }.Where(s => s.Value > 0).ToList();

            return new AnalyticsResponseDto
            {
                Summary = new AnalyticsSummaryDto
                {
                    TotalCandidates = totalCandidates,
                    TotalResumes = totalResumes,
                    TotalMatches = totalMatches,
                    Shortlisted = shortlisted,
                    OpenJobs = openJobs,
                    AverageMatchScore = Math.Round(averageScore, 1)
                },
                HiringFunnel = new List<FunnelStageDto>
                {
                    new() { Stage = "Applied", Count = totalCandidates },
                    new() { Stage = "Parsed", Count = parsedResumes },
                    new() { Stage = "Matched", Count = matchedCandidates },
                    new() { Stage = "Shortlisted", Count = shortlisted },
                    new() { Stage = "On Review", Count = onReview }
                },
                StatusDistribution = statusDistribution,
                GroupedStatusDistribution = groupedStatusDistribution,
                MonthlyApplications = await GetMonthlyResumeUploadsAsync(),
                MonthlyMatchTrend = await GetMonthlyMatchTrendAsync(matchesQuery),
                MonthlyHires = await GetMonthlyHiresAsync(candidatesQuery)
            };
        }

        private async Task<List<MonthlyCountDto>> GetMonthlyResumeUploadsAsync()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);

            var grouped = await _context.Resumes
                .Where(r => r.ParsedAt >= start)
                .GroupBy(r => new { r.ParsedAt.Year, r.ParsedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            return BuildMonthlySeries(start, grouped.Select(g => (g.Year, g.Month, g.Count)));
        }

        private async Task<List<MonthlyTrendDto>> GetMonthlyMatchTrendAsync(
            IQueryable<Models.CandidateJobMatch> matchesQuery)
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);

            var grouped = await matchesQuery
                .Where(m => m.CreatedAt >= start)
                .GroupBy(m => new { m.CreatedAt.Year, m.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Avg = g.Average(x => x.ATSScore)
                })
                .ToListAsync();

            var lookup = grouped.ToDictionary(g => (g.Year, g.Month), g => g.Avg);
            var result = new List<MonthlyTrendDto>();

            for (var i = 0; i < 6; i++)
            {
                var monthDate = start.AddMonths(i);
                lookup.TryGetValue((monthDate.Year, monthDate.Month), out var avg);
                result.Add(new MonthlyTrendDto
                {
                    Month = monthDate.ToString("MMM"),
                    Value = Math.Round(avg, 1)
                });
            }

            return result;
        }

        private async Task<List<MonthlyCountDto>> GetMonthlyHiresAsync(
            IQueryable<Models.Candidate> candidatesQuery)
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-5);

            var grouped = await candidatesQuery
                .Where(c => c.Status == CandidateStatus.Hired && c.UpdatedAt >= start)
                .GroupBy(c => new { c.UpdatedAt.Year, c.UpdatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            return BuildMonthlySeries(start, grouped.Select(g => (g.Year, g.Month, g.Count)));
        }

        private static List<MonthlyCountDto> BuildMonthlySeries(
            DateTime start,
            IEnumerable<(int Year, int Month, int Count)> grouped)
        {
            var lookup = grouped.ToDictionary(g => (g.Year, g.Month), g => g.Count);
            var result = new List<MonthlyCountDto>();

            for (var i = 0; i < 6; i++)
            {
                var monthDate = start.AddMonths(i);
                lookup.TryGetValue((monthDate.Year, monthDate.Month), out var count);
                result.Add(new MonthlyCountDto
                {
                    Month = monthDate.ToString("MMM"),
                    Count = count
                });
            }

            return result;
        }

        private static string FormatStatus(CandidateStatus status) =>
            status switch
            {
                CandidateStatus.HRScreening => "HR Screening",
                CandidateStatus.SentToManager => "Sent to Manager",
                CandidateStatus.InterviewScheduled => "Interview Scheduled",
                CandidateStatus.InterviewCompleted => "Interview Completed",
                CandidateStatus.TechnicalSelected => "Technical Selected",
                CandidateStatus.TechnicalRejected => "Technical Rejected",
                _ => status.ToString()
            };
    }
}
