namespace ResumeAnalyzer.Models
{
    public class Job
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public ICollection<JobSkill> JobSkills { get; set; }
    = new List<JobSkill>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;



    }
}
