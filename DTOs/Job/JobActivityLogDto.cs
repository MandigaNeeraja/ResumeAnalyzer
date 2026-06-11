namespace ResumeAnalyzer.DTOs.Job
{
    public class JobActivityLogDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PerformedByName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
