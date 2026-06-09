namespace ResumeAnalyzer.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalResumes { get; set; }

        public int Shortlisted { get; set; }

        public int OpenJobs { get; set; }

        public int MatchRequests { get; set; }
    }

    public class TopCandidateDto
    {
        public string Name { get; set; } = string.Empty;

        public double Match { get; set; }
    }

    public class RecentActivityDto
    {
        public string Action { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public string Type { get; set; } = string.Empty;
    }

    public class NotificationDto
    {
        public string Message { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }

        public string Type { get; set; } = string.Empty;
    }

    public class NotificationsResponseDto
    {
        public int Count { get; set; }

        public List<NotificationDto> Items { get; set; } = new();
    }

    public class DashboardResponseDto
    {
        public DashboardStatsDto Stats { get; set; } = new();

        public List<TopCandidateDto> TopCandidates { get; set; } = new();

        public List<RecentActivityDto> RecentActivity { get; set; } = new();
    }
}
