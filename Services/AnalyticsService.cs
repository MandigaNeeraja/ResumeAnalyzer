using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Analytics;
using ResumeAnalyzer.Enums;
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

        public async Task<AnalyticsResponseDto> GetAnalyticsAsync()
        {
            var totalCandidates = await _context.Candidates.CountAsync();
            var totalResumes = await _context.Resumes.CountAsync();
            var parsedResumes = await _context.Resumes
                .CountAsync(r => r.ParseStatus == "Parsed");
            var matchedCandidates = await _context.CandidateJobMatches
                .Select(m => m.CandidateId)
                .Distinct()
                .CountAsync();
            var shortlisted = await _context.Candidates
                .CountAsync(c => c.Status == CandidateStatus.Shortlisted);
            var onReview = await _context.Candidates
                .CountAsync(c => c.Status == CandidateStatus.OnHold);
            var totalMatches = await _context.CandidateJobMatches.CountAsync();
            var averageScore = await _context.CandidateJobMatches
                .Select(m => (double?)m.ATSScore)
                .AverageAsync() ?? 0;

            var statusCounts = await _context.Candidates
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var statusDistribution = new List<StatusDistributionDto>();

            var shortlistedCount = statusCounts
                .FirstOrDefault(s => s.Status == CandidateStatus.Shortlisted)?.Count ?? 0;
            var onHoldCount = statusCounts
                .FirstOrDefault(s => s.Status == CandidateStatus.OnHold)?.Count ?? 0;
            var rejectedCount = statusCounts
                .FirstOrDefault(s => s.Status == CandidateStatus.Rejected)?.Count ?? 0;
            var inProgressCount = statusCounts
                .Where(s => s.Status is CandidateStatus.Uploaded or CandidateStatus.Parsed)
                .Sum(s => s.Count);

            if (shortlistedCount > 0)
                statusDistribution.Add(new StatusDistributionDto { Name = "Shortlisted", Value = shortlistedCount });
            if (onHoldCount > 0)
                statusDistribution.Add(new StatusDistributionDto { Name = "Under Review", Value = onHoldCount });
            if (rejectedCount > 0)
                statusDistribution.Add(new StatusDistributionDto { Name = "Not Matched", Value = rejectedCount });
            if (inProgressCount > 0)
                statusDistribution.Add(new StatusDistributionDto { Name = "In Progress", Value = inProgressCount });

            return new AnalyticsResponseDto
            {
                Summary = new AnalyticsSummaryDto
                {
                    TotalCandidates = totalCandidates,
                    TotalResumes = totalResumes,
                    TotalMatches = totalMatches,
                    Shortlisted = shortlisted,
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
                MonthlyApplications = await GetMonthlyApplicationsAsync()
            };
        }

        private async Task<List<MonthlyCountDto>> GetMonthlyApplicationsAsync()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(-5);

            var grouped = await _context.Resumes
                .Where(r => r.ParsedAt >= start)
                .GroupBy(r => new { r.ParsedAt.Year, r.ParsedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new List<MonthlyCountDto>();

            for (var i = 0; i < 6; i++)
            {
                var monthDate = start.AddMonths(i);
                var count = grouped
                    .FirstOrDefault(g =>
                        g.Year == monthDate.Year &&
                        g.Month == monthDate.Month)?.Count ?? 0;

                result.Add(new MonthlyCountDto
                {
                    Month = monthDate.ToString("MMM"),
                    Count = count
                });
            }

            return result;
        }
    }
}
