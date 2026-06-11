namespace ResumeAnalyzer.DTOs.Interview
{
    public class CreateInterviewDto
    {
        public int CandidateId { get; set; }
        public int JobId { get; set; }
        public DateTime InterviewDate { get; set; }
        public TimeSpan InterviewTime { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public string MeetingLink { get; set; } = string.Empty;
    }
}
