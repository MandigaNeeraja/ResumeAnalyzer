namespace ResumeAnalyzer.Models
{
    public class Resume
    {
        public int ResumeId { get; set; }

        public int? CandidateId { get; set; }
        public Candidate? Candidate { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? ExtractedText { get; set; }

        public DateTime ParsedAt { get; set; }

        public string ParseStatus { get; set; } = "Pending";

        public string? ParseErrorMessage { get; set; }
    }
}