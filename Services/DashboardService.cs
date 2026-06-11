using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Analytics;
using ResumeAnalyzer.DTOs.Dashboard;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IAnalyticsService _analyticsService;

        public DashboardService(AppDbContext context, IAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        public async Task<DashboardResponseDto> GetDashboardDataAsync()
        {
            var totalResumes = await _context.Resumes.CountAsync();
            var shortlisted = await _context.Candidates
                .CountAsync(c => c.Status == CandidateStatus.TechnicalSelected);
            var openJobs = await _context.Jobs.CountAsync(j => j.Status == JobStatus.Open);
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

        public async Task<NotificationsResponseDto> GetNotificationsAsync(string? role)
        {
            var items = await BuildNotificationsForRoleAsync(role, 20);

            return new NotificationsResponseDto
            {
                Count = items.Count,
                Items = items
            };
        }

        private async Task<List<RecentActivityDto>> BuildRecentActivityAsync(int take) =>
            (await BuildNotificationsForRoleAsync(null, take))
                .Select(n => new RecentActivityDto
                {
                    Action = n.Message,
                    Detail = n.Detail,
                    OccurredAt = n.OccurredAt,
                    Type = n.Type
                })
                .ToList();

        private async Task<List<NotificationDto>> BuildNotificationsForRoleAsync(string? role, int take)
        {
            var cutoff = DateTime.UtcNow.AddDays(-7);
            var isManager = role == Roles.Manager;
            var notifications = new List<NotificationDto>();

            if (!isManager)
            {
                var recentResumes = await _context.Resumes
                    .Where(r => r.ParsedAt >= cutoff)
                    .OrderByDescending(r => r.ParsedAt)
                    .Take(8)
                    .Join(
                        _context.Candidates,
                        r => r.CandidateId,
                        c => c.CandidateId,
                        (r, c) => new { r, c })
                    .ToListAsync();

                foreach (var item in recentResumes)
                {
                    var link = item.c.JobId.HasValue
                        ? $"/jobs/{item.c.JobId}"
                        : $"/candidate/{item.c.CandidateId}";

                    notifications.Add(new NotificationDto
                    {
                        Id = BuildNotificationId("resume", item.r.ResumeId, item.r.ParsedAt),
                        Message = "Resume parsed",
                        Detail = $"{item.c.FullName} - {item.r.FileName}",
                        OccurredAt = AsUtc(item.r.ParsedAt),
                        Type = "resume",
                        Link = link
                    });
                }

                var recentJobs = await _context.Jobs
                    .Where(j => j.CreatedAt >= cutoff)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                foreach (var job in recentJobs)
                {
                    notifications.Add(new NotificationDto
                    {
                        Id = BuildNotificationId("job", job.JobId, job.CreatedAt),
                        Message = "New job added",
                        Detail = job.Title,
                        OccurredAt = AsUtc(job.CreatedAt),
                        Type = "job",
                        Link = $"/jobs/{job.JobId}"
                    });
                }
            }

            var recentMatches = await _context.CandidateJobMatches
                .Where(m => m.CreatedAt >= cutoff)
                .OrderByDescending(m => m.CreatedAt)
                .Take(8)
                .Join(
                    _context.Candidates,
                    m => m.CandidateId,
                    c => c.CandidateId,
                    (m, c) => new { m, c })
                .Join(
                    _context.Jobs,
                    mc => mc.m.JobId,
                    j => j.JobId,
                    (mc, j) => new { mc.m, mc.c, j })
                .ToListAsync();

            foreach (var item in recentMatches)
            {
                if (isManager && !CandidateVisibilityHelper.IsVisibleToManager(item.c.Status))
                    continue;

                notifications.Add(new NotificationDto
                {
                    Id = BuildNotificationId("match", item.m.CandidateJobMatchId, item.m.CreatedAt),
                    Message = "ATS match completed",
                    Detail = $"{item.c.FullName} matched for {item.j.Title} ({Math.Round(item.m.ATSScore)}%)",
                    OccurredAt = AsUtc(item.m.CreatedAt),
                    Type = "match",
                    Link = $"/jobs/{item.j.JobId}/candidates/{item.c.CandidateId}"
                });
            }

            var workflowCandidates = await _context.Candidates
                .Where(c => c.UpdatedAt >= cutoff &&
                            c.Status != CandidateStatus.Applied)
                .Include(c => c.Job)
                .OrderByDescending(c => c.UpdatedAt)
                .Take(12)
                .ToListAsync();

            foreach (var candidate in workflowCandidates)
            {
                if (isManager && !CandidateVisibilityHelper.IsVisibleToManager(candidate.Status))
                    continue;

                var workflow = MapWorkflowNotification(candidate, role);
                if (workflow == null)
                    continue;

                notifications.Add(workflow);
            }

            if (!isManager)
            {
                var recentInterviews = await _context.Interviews
                    .Where(i => i.CreatedAt >= cutoff)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(6)
                    .Join(
                        _context.Candidates,
                        i => i.CandidateId,
                        c => c.CandidateId,
                        (i, c) => new { i, c })
                    .Join(
                        _context.Jobs,
                        ic => ic.i.JobId,
                        j => j.JobId,
                        (ic, j) => new { ic.i, ic.c, j })
                    .ToListAsync();

                foreach (var item in recentInterviews)
                {
                    notifications.Add(new NotificationDto
                    {
                        Id = BuildNotificationId("interview", item.i.InterviewId, item.i.CreatedAt),
                        Message = "Interview scheduled",
                        Detail = $"{item.c.FullName} - {item.j.Title}",
                        OccurredAt = AsUtc(item.i.CreatedAt),
                        Type = "interview",
                        Link = "/interviews"
                    });
                }
            }

            if (isManager || role == Roles.Admin)
            {
                var recentFeedback = await _context.InterviewFeedbacks
                    .Where(f => f.CreatedAt >= cutoff)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(5)
                    .Join(
                        _context.Candidates,
                        f => f.CandidateId,
                        c => c.CandidateId,
                        (f, c) => new { f, c })
                    .ToListAsync();

                foreach (var item in recentFeedback)
                {
                    if (isManager && !CandidateVisibilityHelper.IsVisibleToManager(item.c.Status))
                        continue;

                    notifications.Add(new NotificationDto
                    {
                        Id = BuildNotificationId("feedback", item.f.FeedbackId, item.f.CreatedAt),
                        Message = "Interview feedback submitted",
                        Detail = $"{item.c.FullName} - {item.f.Decision}",
                        OccurredAt = AsUtc(item.f.CreatedAt),
                        Type = "feedback",
                        Link = $"/candidate/{item.c.CandidateId}"
                    });
                }
            }

            return notifications
                .GroupBy(n => n.Id)
                .Select(g => g.First())
                .OrderByDescending(n => n.OccurredAt)
                .Take(take)
                .ToList();
        }

        private static NotificationDto? MapWorkflowNotification(Models.Candidate candidate, string? role)
        {
            var jobTitle = candidate.Job?.Title ?? "Open role";
            var link = candidate.JobId.HasValue
                ? $"/jobs/{candidate.JobId}/candidates/{candidate.CandidateId}"
                : $"/candidate/{candidate.CandidateId}";

            return candidate.Status switch
            {
                CandidateStatus.HRScreening when role != Roles.Manager =>
                    new NotificationDto
                    {
                        Id = BuildNotificationId("screening", candidate.CandidateId, candidate.UpdatedAt),
                        Message = "Candidate ready for HR screening",
                        Detail = $"{candidate.FullName} - {jobTitle}",
                        OccurredAt = AsUtc(candidate.UpdatedAt),
                        Type = "screening",
                        Link = "/hr-screening"
                    },
                CandidateStatus.SentToManager when role is Roles.Manager or Roles.Admin =>
                    new NotificationDto
                    {
                        Id = BuildNotificationId("review", candidate.CandidateId, candidate.UpdatedAt),
                        Message = "Candidate sent for your review",
                        Detail = $"{candidate.FullName} - {jobTitle}",
                        OccurredAt = AsUtc(candidate.UpdatedAt),
                        Type = "review",
                        Link = "/manager-review"
                    },
                CandidateStatus.InterviewScheduled =>
                    new NotificationDto
                    {
                        Id = BuildNotificationId("interview-status", candidate.CandidateId, candidate.UpdatedAt),
                        Message = "Interview stage started",
                        Detail = $"{candidate.FullName} - {jobTitle}",
                        OccurredAt = AsUtc(candidate.UpdatedAt),
                        Type = "interview",
                        Link = "/interviews"
                    },
                CandidateStatus.TechnicalSelected when role != Roles.Manager =>
                    new NotificationDto
                    {
                        Id = BuildNotificationId("selected", candidate.CandidateId, candidate.UpdatedAt),
                        Message = "Candidate technically selected",
                        Detail = $"{candidate.FullName} - {jobTitle}",
                        OccurredAt = AsUtc(candidate.UpdatedAt),
                        Type = "candidate",
                        Link = "/hiring"
                    },
                CandidateStatus.Hired when role != Roles.Manager =>
                    new NotificationDto
                    {
                        Id = BuildNotificationId("hired", candidate.CandidateId, candidate.UpdatedAt),
                        Message = "Candidate hired",
                        Detail = $"{candidate.FullName} - {jobTitle}",
                        OccurredAt = AsUtc(candidate.UpdatedAt),
                        Type = "hired",
                        Link = "/hiring"
                    },
                CandidateStatus.TechnicalRejected when role is Roles.Manager or Roles.Admin =>
                    new NotificationDto
                    {
                        Id = BuildNotificationId("rejected", candidate.CandidateId, candidate.UpdatedAt),
                        Message = "Candidate technically rejected",
                        Detail = $"{candidate.FullName} - {jobTitle}",
                        OccurredAt = AsUtc(candidate.UpdatedAt),
                        Type = "candidate",
                        Link = link
                    },
                _ => null
            };
        }

        private static string BuildNotificationId(string type, int entityId, DateTime occurredAt) =>
            $"{type}-{entityId}-{occurredAt.Ticks}";

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var insights = await BuildDashboardInsightsAsync(Roles.Admin);
            return new AdminDashboardDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalCandidates = await _context.Candidates.CountAsync(),
                TotalInterviews = await _context.Interviews.CountAsync(),
                Analytics = insights.Analytics,
                RecentActivity = insights.RecentActivity,
                TopCandidates = insights.TopCandidates
            };
        }

        public async Task<HRDashboardDto> GetHRDashboardAsync()
        {
            var insights = await BuildDashboardInsightsAsync(Roles.HR);
            return new HRDashboardDto
            {
                TotalJobs = await _context.Jobs.CountAsync(),
                CandidatesUploaded = await _context.Candidates.CountAsync(),
                CandidatesUnderScreening = await _context.Candidates
                    .CountAsync(c => c.Status == CandidateStatus.HRScreening),
                ScheduledInterviews = await _context.Interviews
                    .CountAsync(i => i.Status == InterviewStatus.Scheduled),
                SelectedCandidates = await _context.Candidates
                    .CountAsync(c => c.Status == CandidateStatus.TechnicalSelected ||
                                     c.Status == CandidateStatus.Hired),
                Analytics = insights.Analytics,
                RecentActivity = insights.RecentActivity,
                TopCandidates = insights.TopCandidates
            };
        }

        public async Task<ManagerDashboardDto> GetManagerDashboardAsync()
        {
            var insights = await BuildDashboardInsightsAsync(Roles.Manager);
            return new ManagerDashboardDto
            {
                CandidatesPendingReview = await _context.Candidates
                    .CountAsync(c => c.Status == CandidateStatus.SentToManager),
                InterviewsPending = await _context.Interviews
                    .CountAsync(i => i.Status == InterviewStatus.Scheduled),
                SelectedCandidates = await _context.Candidates
                    .CountAsync(c => c.Status == CandidateStatus.TechnicalSelected),
                RejectedCandidates = await _context.Candidates
                    .CountAsync(c => c.Status == CandidateStatus.TechnicalRejected ||
                                     c.Status == CandidateStatus.Rejected),
                Analytics = insights.Analytics,
                RecentActivity = insights.RecentActivity,
                TopCandidates = insights.TopCandidates
            };
        }

        private async Task<(AnalyticsResponseDto Analytics,
            List<RecentActivityDto> RecentActivity,
            List<TopCandidateDto> TopCandidates)> BuildDashboardInsightsAsync(string role)
        {
            var analytics = await _analyticsService.GetAnalyticsAsync(role);
            var recentActivity = (await BuildNotificationsForRoleAsync(role, 8))
                .Select(n => new RecentActivityDto
                {
                    Action = n.Message,
                    Detail = n.Detail,
                    OccurredAt = n.OccurredAt,
                    Type = n.Type
                })
                .ToList();

            var topCandidates = await GetTopCandidatesAsync(5, role);
            return (analytics, recentActivity, topCandidates);
        }

        private async Task<List<TopCandidateDto>> GetTopCandidatesAsync(int take, string role)
        {
            var matchesQuery = _context.CandidateJobMatches.AsQueryable();

            if (role == Roles.Manager)
            {
                matchesQuery = matchesQuery.Where(m =>
                    _context.Candidates.Any(c =>
                        c.CandidateId == m.CandidateId &&
                        CandidateVisibilityHelper.ManagerVisibleStatuses.Contains(c.Status)));
            }

            return await matchesQuery
                .OrderByDescending(m => m.ATSScore)
                .Take(take)
                .Join(
                    _context.Candidates,
                    m => m.CandidateId,
                    c => c.CandidateId,
                    (m, c) => new TopCandidateDto
                    {
                        Name = c.FullName,
                        Match = Math.Round(m.ATSScore, 1)
                    })
                .ToListAsync();
        }

        private static DateTime AsUtc(DateTime dateTime) =>
            dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
