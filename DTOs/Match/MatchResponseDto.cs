namespace ResumeAnalyzer.DTOs.Match
{
    public class MatchResponseDto
    {
        public int CandidateId { get; set; }

        public int JobId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public List<string> SkillsMatched { get; set; } = new();

        public double MatchScore { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
