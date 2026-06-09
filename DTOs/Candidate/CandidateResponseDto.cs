namespace ResumeAnalyzer.DTOs.Candidate
{
    public class CandidateResponseDto
    {
        public int CandidateId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Experience { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();

        public string Status { get; set; } = string.Empty;
    }
}
