using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Interview;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Repositories;

namespace ResumeAnalyzer.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewRepository _interviewRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly AppDbContext _context;

        public InterviewService(
            IInterviewRepository interviewRepository,
            ICandidateRepository candidateRepository,
            AppDbContext context)
        {
            _interviewRepository = interviewRepository;
            _candidateRepository = candidateRepository;
            _context = context;
        }

        public async Task<InterviewResponseDto> CreateInterviewAsync(
            CreateInterviewDto dto,
            int scheduledByUserId)
        {
            var candidate = await _candidateRepository.GetByIdAsync(dto.CandidateId)
                ?? throw new InvalidOperationException("Candidate not found");

            if (candidate.Status != CandidateStatus.InterviewScheduled)
                throw new InvalidOperationException(
                    "Candidate must be in InterviewScheduled status before scheduling an interview");

            if (ManagerAvailabilityHelper.HasAvailability(candidate.ManagerAvailability) &&
                !ManagerAvailabilityHelper.IsScheduledWithinAvailability(
                    candidate.ManagerAvailability,
                    dto.InterviewDate,
                    dto.InterviewTime))
            {
                throw new InvalidOperationException(
                    "Interview must be scheduled within the manager's available time slots");
            }

            if (!Enum.TryParse<InterviewType>(dto.InterviewType, true, out var interviewType))
                throw new InvalidOperationException("Invalid interview type");

            var interview = new Interview
            {
                CandidateId = dto.CandidateId,
                JobId = dto.JobId,
                InterviewDate = dto.InterviewDate,
                InterviewTime = dto.InterviewTime,
                InterviewType = interviewType,
                MeetingLink = dto.MeetingLink,
                Status = InterviewStatus.Scheduled,
                ScheduledBy = scheduledByUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _interviewRepository.AddAsync(interview);
            await _interviewRepository.SaveChangesAsync();

            await JobActivityHelper.LogAsync(
                _context,
                dto.JobId,
                JobActivityType.InterviewScheduled,
                $"Interview scheduled for {candidate.FullName} on {dto.InterviewDate:yyyy-MM-dd}.",
                scheduledByUserId,
                dto.CandidateId,
                interview.InterviewId);

            var created = await _interviewRepository.GetWithDetailsAsync(interview.InterviewId);
            return MapToDto(created!);
        }

        public async Task<List<InterviewResponseDto>> GetInterviewsAsync()
        {
            var interviews = await _interviewRepository.GetAllWithDetailsAsync();
            return interviews.Select(MapToDto).ToList();
        }

        public async Task<InterviewResponseDto?> GetInterviewByIdAsync(int id)
        {
            var interview = await _interviewRepository.GetWithDetailsAsync(id);
            return interview == null ? null : MapToDto(interview);
        }

        public async Task<InterviewResponseDto?> UpdateInterviewAsync(
            int id,
            UpdateInterviewDto dto)
        {
            var interview = await _interviewRepository.GetWithDetailsAsync(id);
            if (interview == null)
                return null;

            if (!Enum.TryParse<InterviewType>(dto.InterviewType, true, out var interviewType))
                throw new InvalidOperationException("Invalid interview type");

            if (!Enum.TryParse<InterviewStatus>(dto.Status, true, out var status))
                throw new InvalidOperationException("Invalid interview status");

            interview.InterviewDate = dto.InterviewDate;
            interview.InterviewTime = dto.InterviewTime;
            interview.InterviewType = interviewType;
            interview.MeetingLink = dto.MeetingLink;
            interview.Status = status;

            _interviewRepository.Update(interview);
            await _interviewRepository.SaveChangesAsync();

            return MapToDto(interview);
        }

        public async Task<InterviewResponseDto?> CompleteInterviewAsync(int id)
        {
            var interview = await _interviewRepository.GetWithDetailsAsync(id);
            if (interview == null)
                return null;

            interview.Status = InterviewStatus.Completed;
            _interviewRepository.Update(interview);

            var candidate = await _candidateRepository.GetByIdAsync(interview.CandidateId);
            if (candidate != null)
            {
                candidate.Status = CandidateStatus.InterviewCompleted;
                candidate.UpdatedAt = DateTime.UtcNow;
                _candidateRepository.Update(candidate);
            }

            await _interviewRepository.SaveChangesAsync();

            return MapToDto(interview);
        }

        private static InterviewResponseDto MapToDto(Interview interview) =>
            new()
            {
                InterviewId = interview.InterviewId,
                CandidateId = interview.CandidateId,
                CandidateName = interview.Candidate?.FullName ?? "",
                JobId = interview.JobId,
                JobTitle = interview.Job?.Title ?? "",
                InterviewDate = interview.InterviewDate,
                InterviewTime = interview.InterviewTime,
                InterviewType = interview.InterviewType.ToString(),
                MeetingLink = interview.MeetingLink,
                Status = interview.Status.ToString(),
                ScheduledBy = interview.ScheduledBy,
                ScheduledByName = interview.Scheduler?.Name,
                CreatedDate = interview.CreatedAt
            };
    }
}
