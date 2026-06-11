using ResumeAnalyzer.DTOs.InterviewFeedback;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Repositories;

namespace ResumeAnalyzer.Services
{
    public class InterviewFeedbackService : IInterviewFeedbackService
    {
        private readonly IInterviewFeedbackRepository _feedbackRepository;
        private readonly IInterviewRepository _interviewRepository;
        private readonly ICandidateRepository _candidateRepository;

        public InterviewFeedbackService(
            IInterviewFeedbackRepository feedbackRepository,
            IInterviewRepository interviewRepository,
            ICandidateRepository candidateRepository)
        {
            _feedbackRepository = feedbackRepository;
            _interviewRepository = interviewRepository;
            _candidateRepository = candidateRepository;
        }

        public async Task<InterviewFeedbackResponseDto> CreateFeedbackAsync(
            CreateInterviewFeedbackDto dto,
            int managerId)
        {
            var interview = await _interviewRepository.GetWithDetailsAsync(dto.InterviewId)
                ?? throw new InvalidOperationException("Interview not found");

            if (interview.Status != InterviewStatus.Completed)
                throw new InvalidOperationException(
                    "Feedback can only be submitted for completed interviews");

            var existing = await _feedbackRepository.GetByInterviewIdAsync(dto.InterviewId);
            if (existing != null)
                throw new InvalidOperationException("Feedback already submitted for this interview");

            if (!Enum.TryParse<FeedbackDecision>(dto.Decision, true, out var decision))
                throw new InvalidOperationException("Invalid decision value");

            var feedback = new InterviewFeedback
            {
                InterviewId = dto.InterviewId,
                CandidateId = dto.CandidateId,
                ManagerId = managerId,
                TechnicalKnowledgeRating = dto.TechnicalKnowledgeRating,
                ProblemSolvingRating = dto.ProblemSolvingRating,
                CommunicationRating = dto.CommunicationRating,
                Comments = dto.Comments,
                Decision = decision,
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddAsync(feedback);

            var candidate = await _candidateRepository.GetByIdAsync(dto.CandidateId);
            if (candidate != null)
            {
                candidate.Status = decision == FeedbackDecision.Selected
                    ? CandidateStatus.TechnicalSelected
                    : CandidateStatus.TechnicalRejected;
                candidate.UpdatedAt = DateTime.UtcNow;
                _candidateRepository.Update(candidate);
            }

            await _feedbackRepository.SaveChangesAsync();

            var created = await _feedbackRepository.GetByInterviewIdAsync(dto.InterviewId);
            return MapToDto(created ?? feedback);
        }

        public async Task<List<InterviewFeedbackResponseDto>> GetFeedbackByCandidateIdAsync(
            int candidateId)
        {
            var feedbacks = await _feedbackRepository.GetByCandidateIdAsync(candidateId);
            return feedbacks.Select(MapToDto).ToList();
        }

        private static InterviewFeedbackResponseDto MapToDto(InterviewFeedback feedback) =>
            new()
            {
                FeedbackId = feedback.FeedbackId,
                InterviewId = feedback.InterviewId,
                CandidateId = feedback.CandidateId,
                ManagerId = feedback.ManagerId,
                ManagerName = feedback.Manager?.Name ?? "",
                TechnicalKnowledgeRating = feedback.TechnicalKnowledgeRating,
                ProblemSolvingRating = feedback.ProblemSolvingRating,
                CommunicationRating = feedback.CommunicationRating,
                Comments = feedback.Comments,
                Decision = feedback.Decision.ToString(),
                CreatedDate = feedback.CreatedAt
            };
    }
}
