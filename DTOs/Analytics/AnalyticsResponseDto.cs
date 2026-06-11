namespace ResumeAnalyzer.DTOs.Analytics
{
    public class FunnelStageDto
    {
        public string Stage { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class StatusDistributionDto
    {
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }
    }

    public class MonthlyCountDto
    {
        public string Month { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class MonthlyTrendDto
    {
        public string Month { get; set; } = string.Empty;

        public double Value { get; set; }
    }

    public class AnalyticsSummaryDto
    {
        public int TotalCandidates { get; set; }

        public int TotalResumes { get; set; }

        public int TotalMatches { get; set; }

        public int Shortlisted { get; set; }

        public int OpenJobs { get; set; }

        public double AverageMatchScore { get; set; }
    }

    public class AnalyticsResponseDto
    {
        public AnalyticsSummaryDto Summary { get; set; } = new();

        public List<FunnelStageDto> HiringFunnel { get; set; } = new();

        public List<StatusDistributionDto> StatusDistribution { get; set; } = new();

        public List<StatusDistributionDto> GroupedStatusDistribution { get; set; } = new();

        public List<MonthlyCountDto> MonthlyApplications { get; set; } = new();

        public List<MonthlyTrendDto> MonthlyMatchTrend { get; set; } = new();

        public List<MonthlyCountDto> MonthlyHires { get; set; } = new();
    }
}
