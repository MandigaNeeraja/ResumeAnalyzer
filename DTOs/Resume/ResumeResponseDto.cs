namespace ResumeAnalyzer.DTOs.Resume
{
    public class ResumeResponseDto
    {
        public int ResumeId { get; set; }

        public int CandidateId { get; set; }

        public int? JobId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string? LinkedIn { get; set; }

        public List<string> Skills { get; set; } = new();

        public string ParseStatus { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public DateTime UploadedOn { get; set; }

        public double? MatchScore { get; set; }

        public List<string> SkillsMatched { get; set; } = new();

        public string? MatchStatus { get; set; }
    }
}
