using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Models
{
    public class Interview
    {
        public int InterviewId { get; set; }

        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;

        public int JobId { get; set; }
        public Job Job { get; set; } = null!;

        public DateTime InterviewDate { get; set; }
        public TimeSpan InterviewTime { get; set; }
        public InterviewType InterviewType { get; set; }
        public string MeetingLink { get; set; } = string.Empty;
        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        public int? ScheduledBy { get; set; }
        public User? Scheduler { get; set; }

        public InterviewFeedback? Feedback { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
