namespace ResumeAnalyzer.DTOs.Job
{
    public class JobCandidateSummaryDto
    {
        public int All { get; set; }
        public int Shortlisted { get; set; }
        public int OnHold { get; set; }
        public int Rejected { get; set; }
        public int SentToManager { get; set; }
        public int Selected { get; set; }
    }
}
