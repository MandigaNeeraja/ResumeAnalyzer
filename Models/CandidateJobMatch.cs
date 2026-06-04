namespace ResumeAnalyzer.Models
{
    public class CandidateJobMatch
    {
        public int CandidateJobMatchId { get; set; }

        public int CandidateId { get; set; }

        public int JobId { get; set; }

        public double ATSScore { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}