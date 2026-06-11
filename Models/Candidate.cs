using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Models
{
    public class Candidate
    {
        public int CandidateId { get; set; }

        public int? JobId { get; set; }
        public Job? Job { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? LinkedIn { get; set; }

        public int? ExperienceYears { get; set; }
        public string? Education { get; set; }
        public string? CurrentDesignation { get; set; }

        public double? ATSScore { get; set; }
        public CandidateStatus Status { get; set; } = CandidateStatus.Applied;

        public string? HRRemarks { get; set; }
        public string? ManagerRemarks { get; set; }
        public string? ManagerAvailability { get; set; }

        public Resume? Resume { get; set; }
        public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
        public ICollection<InterviewFeedback> InterviewFeedbacks { get; set; } = new List<InterviewFeedback>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
