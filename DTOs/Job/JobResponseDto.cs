namespace ResumeAnalyzer.DTOs.Job
{
    public class JobResponseDto
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public int ResumeCount { get; set; }

        public int MatchCount { get; set; }
    }
}
