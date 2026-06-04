namespace ResumeAnalyzer.Models
{
    public class CandidateSkill
    {
        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;
        public int SkillId { get; set; }
        public Skill Skill { get; set; }= null!;

        public string Source { get; set; } = "Resume";
    }
}
