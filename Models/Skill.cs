using ResumeAnalyzer.Models;

public class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public ICollection<CandidateSkill> CandidateSkills
        = new List<CandidateSkill>();

    public ICollection<JobSkill> JobSkills
        = new List<JobSkill>();
}