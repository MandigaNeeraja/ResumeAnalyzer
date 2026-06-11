namespace ResumeAnalyzer.DTOs.Interview
{
    public class UpdateInterviewDto
    {
        public DateTime InterviewDate { get; set; }
        public TimeSpan InterviewTime { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public string MeetingLink { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
