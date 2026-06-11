using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Repositories
{
    public interface IInterviewFeedbackRepository : IRepository<InterviewFeedback>
    {
        Task<IReadOnlyList<InterviewFeedback>> GetByCandidateIdAsync(int candidateId);
        Task<InterviewFeedback?> GetByInterviewIdAsync(int interviewId);
    }
}
