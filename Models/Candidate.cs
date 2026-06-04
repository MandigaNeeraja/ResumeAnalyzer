using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Models
{
    public class Candidate
    {
        public int CandidateId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string?  Email { get; set; }
        public string Phone { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Education { get; set; }

        public string? CurrentDesignation { get; set; }

        public CandidateStatus Status { get; set; }= CandidateStatus.Uploaded;
        public Resume? Resume { get; set; }

        public ICollection<CandidateSkill> CandidateSkills { get; set; }= new List<CandidateSkill>();

        //public int score { get; set; }
        //status

        
        public DateTime CreatedAt { get; set; }= DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
