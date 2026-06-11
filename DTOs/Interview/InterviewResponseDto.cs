namespace ResumeAnalyzer.DTOs.Interview
{
    public class InterviewResponseDto
    {
        public int InterviewId { get; set; }
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public DateTime InterviewDate { get; set; }
        public TimeSpan InterviewTime { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public string MeetingLink { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ScheduledBy { get; set; }
        public string? ScheduledByName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
