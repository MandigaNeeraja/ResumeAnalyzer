namespace ResumeAnalyzer.DTOs.Job
{
    public class CreateJobDto
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string ExperienceRequired { get; set; } = string.Empty;
        public List<string> RequiredSkills { get; set; } = new();
        public string Description { get; set; } = string.Empty;
    }
}
