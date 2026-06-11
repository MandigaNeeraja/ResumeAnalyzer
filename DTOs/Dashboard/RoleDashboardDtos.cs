using ResumeAnalyzer.DTOs.Analytics;

namespace ResumeAnalyzer.DTOs.Dashboard
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalJobs { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalInterviews { get; set; }
        public AnalyticsResponseDto Analytics { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public List<TopCandidateDto> TopCandidates { get; set; } = new();
    }

    public class HRDashboardDto
    {
        public int TotalJobs { get; set; }
        public int CandidatesUploaded { get; set; }
        public int CandidatesUnderScreening { get; set; }
        public int ScheduledInterviews { get; set; }
        public int SelectedCandidates { get; set; }
        public AnalyticsResponseDto Analytics { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public List<TopCandidateDto> TopCandidates { get; set; } = new();
    }

    public class ManagerDashboardDto
    {
        public int CandidatesPendingReview { get; set; }
        public int InterviewsPending { get; set; }
        public int SelectedCandidates { get; set; }
        public int RejectedCandidates { get; set; }
        public AnalyticsResponseDto Analytics { get; set; } = new();
        public List<RecentActivityDto> RecentActivity { get; set; } = new();
        public List<TopCandidateDto> TopCandidates { get; set; } = new();
    }
}
