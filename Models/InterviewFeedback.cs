using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Models
{
    public class InterviewFeedback
    {
        public int FeedbackId { get; set; }

        public int InterviewId { get; set; }
        public Interview Interview { get; set; } = null!;

        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;

        public int ManagerId { get; set; }
        public User Manager { get; set; } = null!;

        public int TechnicalKnowledgeRating { get; set; }
        public int ProblemSolvingRating { get; set; }
        public int CommunicationRating { get; set; }

        public string Comments { get; set; } = string.Empty;
        public FeedbackDecision Decision { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
