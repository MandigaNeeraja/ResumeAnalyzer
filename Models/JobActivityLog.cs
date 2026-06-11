using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Models
{
    public class JobActivityLog
    {
        public int JobActivityLogId { get; set; }
        public int JobId { get; set; }
        public Job Job { get; set; } = null!;
        public JobActivityType ActivityType { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? PerformedBy { get; set; }
        public User? Performer { get; set; }
        public int? RelatedCandidateId { get; set; }
        public int? RelatedInterviewId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
