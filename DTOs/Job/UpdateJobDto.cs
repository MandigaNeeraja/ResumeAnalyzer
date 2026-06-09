namespace ResumeAnalyzer.DTOs.Job
{
    public class UpdateJobDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();
    }
}
