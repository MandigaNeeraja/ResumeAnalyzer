namespace ResumeAnalyzer.Models
{
    public class CandidateJobMatch
    {
        public int CandidateJobMatchId { get; set; }

        public int CandidateId { get; set; }
        public Candidate Candidate { get; set; } = null!;

        public int JobId { get; set; }
        public Job Job { get; set; } = null!;

        public double ATSScore { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
