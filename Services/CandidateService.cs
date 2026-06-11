using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Candidate;
using ResumeAnalyzer.DTOs.Match;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Repositories;
using System.Security.Claims;

namespace ResumeAnalyzer.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;
        private readonly IMatchService _matchService;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CandidateService(
            ICandidateRepository candidateRepository,
            IMatchService matchService,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _candidateRepository = candidateRepository;
            _matchService = matchService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<CandidateResponseDto>> GetCandidatesAsync()
        {
            var candidates = await _candidateRepository.GetAllWithDetailsAsync();
            return candidates.Select(CandidateMapper.ToDto).ToList();
        }

        public async Task<List<CandidateResponseDto>> GetCandidatesForRoleAsync(string role)
        {
            if (role == Roles.Manager)
            {
                var managerCandidates = await _candidateRepository
                    .GetByStatusesAsync(CandidateVisibilityHelper.ManagerVisibleStatuses);
                return managerCandidates.Select(CandidateMapper.ToDto).ToList();
            }

            return await GetCandidatesAsync();
        }

        public async Task<CandidateResponseDto?> GetCandidateByIdAsync(int id)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            return candidate == null ? null : CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> GetCandidateByIdForRoleAsync(int id, string role)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null)
                return null;

            if (!CandidateVisibilityHelper.CanAccessCandidate(role, candidate.Status))
                return null;

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<MatchResponseDto?> GetCandidateMatchForJobAsync(
            int candidateId,
            int jobId) =>
            await _matchService.GetMatchAsync(candidateId, jobId);

        public async Task<List<CandidateResponseDto>> GetHrScreeningCandidatesAsync()
        {
            var candidates = await _candidateRepository
                .GetByStatusesAsync(CandidateVisibilityHelper.HrScreeningStatuses);
            return candidates.Select(CandidateMapper.ToDto).ToList();
        }

        public async Task<List<CandidateResponseDto>> GetManagerReviewCandidatesAsync()
        {
            var candidates = await _candidateRepository
                .GetByStatusAsync(CandidateStatus.SentToManager);
            return candidates.Select(CandidateMapper.ToDto).ToList();
        }

        public async Task<CandidateResponseDto?> SendToManagerAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null ||
                (candidate.Status != CandidateStatus.HRScreening &&
                 candidate.Status != CandidateStatus.Shortlisted &&
                 candidate.Status != CandidateStatus.OnHold))
                return null;

            candidate.Status = CandidateStatus.SentToManager;
            candidate.HRRemarks = dto.Remarks ?? candidate.HRRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            await LogCandidateActivityAsync(
                candidate, JobActivityType.SentToManager,
                $"{candidate.FullName} sent to manager for review.");

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> ShortlistCandidateAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null ||
                (candidate.Status != CandidateStatus.HRScreening &&
                 candidate.Status != CandidateStatus.Applied))
                return null;

            candidate.Status = CandidateStatus.Shortlisted;
            candidate.HRRemarks = dto.Remarks ?? candidate.HRRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            await LogCandidateActivityAsync(
                candidate, JobActivityType.CandidateShortlisted,
                $"{candidate.FullName} shortlisted by HR.");

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> RejectCandidateAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null)
                return null;

            var allowed = candidate.Status is CandidateStatus.HRScreening
                or CandidateStatus.Shortlisted
                or CandidateStatus.OnHold
                or CandidateStatus.SentToManager;

            if (!allowed)
                return null;

            var isManager = candidate.Status == CandidateStatus.SentToManager;
            candidate.Status = CandidateStatus.Rejected;
            if (isManager)
                candidate.ManagerRemarks = dto.Remarks ?? candidate.ManagerRemarks;
            else
                candidate.HRRemarks = dto.Remarks ?? candidate.HRRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            await LogCandidateActivityAsync(
                candidate, JobActivityType.CandidateRejected,
                $"{candidate.FullName} rejected.");

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> HoldCandidateAsync(
            int id,
            CandidateWorkflowDto dto,
            bool isManager)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null)
                return null;

            if (isManager)
            {
                if (candidate.Status != CandidateStatus.SentToManager)
                    return null;
                candidate.ManagerRemarks = dto.Remarks ?? candidate.ManagerRemarks;
            }
            else
            {
                if (candidate.Status is not (CandidateStatus.HRScreening or CandidateStatus.Shortlisted))
                    return null;
                candidate.HRRemarks = dto.Remarks ?? candidate.HRRemarks;
            }

            candidate.Status = CandidateStatus.OnHold;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            await LogCandidateActivityAsync(
                candidate, JobActivityType.CandidateOnHold,
                $"{candidate.FullName} put on hold.");

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> ResumeToScreeningAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null || candidate.Status != CandidateStatus.OnHold)
                return null;

            candidate.Status = CandidateStatus.HRScreening;
            candidate.HRRemarks = dto.Remarks ?? candidate.HRRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> ResumeToManagerReviewAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null || candidate.Status != CandidateStatus.OnHold)
                return null;

            candidate.Status = CandidateStatus.SentToManager;
            candidate.ManagerRemarks = dto.Remarks ?? candidate.ManagerRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> ApproveInterviewAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null || candidate.Status != CandidateStatus.SentToManager)
                return null;

            if (dto.AvailabilitySlots == null || dto.AvailabilitySlots.Count == 0)
                throw new InvalidOperationException("At least one manager availability slot is required");

            candidate.Status = CandidateStatus.InterviewScheduled;
            candidate.ManagerRemarks = dto.Remarks ?? candidate.ManagerRemarks;
            candidate.ManagerAvailability = ManagerAvailabilityHelper.Serialize(dto.AvailabilitySlots);
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> TechnicalSelectAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null ||
                (candidate.Status != CandidateStatus.InterviewCompleted &&
                 candidate.Status != CandidateStatus.InterviewScheduled))
                return null;

            candidate.Status = CandidateStatus.TechnicalSelected;
            candidate.ManagerRemarks = dto.Remarks ?? candidate.ManagerRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> TechnicalRejectAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null ||
                (candidate.Status != CandidateStatus.InterviewCompleted &&
                 candidate.Status != CandidateStatus.InterviewScheduled))
                return null;

            candidate.Status = CandidateStatus.TechnicalRejected;
            candidate.ManagerRemarks = dto.Remarks ?? candidate.ManagerRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            return CandidateMapper.ToDto(candidate);
        }

        public async Task<CandidateResponseDto?> HireAsync(
            int id,
            CandidateWorkflowDto dto)
        {
            var candidate = await _candidateRepository.GetWithDetailsAsync(id);
            if (candidate == null || candidate.Status != CandidateStatus.TechnicalSelected)
                return null;

            candidate.Status = CandidateStatus.Hired;
            candidate.HRRemarks = dto.Remarks ?? candidate.HRRemarks;
            candidate.UpdatedAt = DateTime.UtcNow;

            _candidateRepository.Update(candidate);
            await _candidateRepository.SaveChangesAsync();

            return CandidateMapper.ToDto(candidate);
        }

        private async Task LogCandidateActivityAsync(
            Models.Candidate candidate,
            JobActivityType type,
            string description)
        {
            if (candidate.JobId == null)
                return;

            var userId = await GetCurrentUserIdAsync();
            await JobActivityHelper.LogAsync(
                _context,
                candidate.JobId.Value,
                type,
                description,
                userId,
                candidate.CandidateId);
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            return user?.UserId;
        }
    }
}
