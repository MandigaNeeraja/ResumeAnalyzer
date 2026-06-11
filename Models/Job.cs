using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Models
{
    public class Job
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int? CreatedBy { get; set; }
        public User? Creator { get; set; }

        public ICollection<JobSkill> JobSkills { get; set; } = new List<JobSkill>();
        public ICollection<Candidate> Candidates { get; set; } = new List<Candidate>();
        public ICollection<Interview> Interviews { get; set; } = new List<Interview>();
        public ICollection<JobActivityLog> ActivityLogs { get; set; } = new List<JobActivityLog>();

        public JobStatus Status { get; set; } = JobStatus.Open;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
