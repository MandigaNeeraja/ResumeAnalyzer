namespace ResumeAnalyzer.DTOs.Job
{
    public class JobResponseDto
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string ExperienceRequired { get; set; } = string.Empty;
        public List<string> RequiredSkills { get; set; } = new();
        public string Description { get; set; } = string.Empty;
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ResumeCount { get; set; }
        public int MatchCount { get; set; }
        public string Status { get; set; } = "Open";
        public DateTime? UpdatedDate { get; set; }
    }
}
