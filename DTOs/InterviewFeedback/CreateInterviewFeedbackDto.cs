namespace ResumeAnalyzer.DTOs.InterviewFeedback
{
    public class CreateInterviewFeedbackDto
    {
        public int InterviewId { get; set; }
        public int CandidateId { get; set; }
        public int TechnicalKnowledgeRating { get; set; }
        public int ProblemSolvingRating { get; set; }
        public int CommunicationRating { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string Decision { get; set; } = string.Empty;
    }
}
