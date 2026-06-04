namespace ResumeAnalyzer.DTOs.Job
{
    public class JobResponseDto
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}