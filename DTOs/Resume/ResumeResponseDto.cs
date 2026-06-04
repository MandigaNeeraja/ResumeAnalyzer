namespace ResumeAnalyzer.DTOs.Resume
{
    public class ResumeResponseDto
    {
        public int ResumeId { get; set; }

        public int CandidateId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();

        public string ParseStatus { get; set; } = string.Empty;
    }
}