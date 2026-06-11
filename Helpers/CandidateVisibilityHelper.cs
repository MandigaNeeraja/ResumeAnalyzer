using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Helpers
{
    public static class CandidateVisibilityHelper
    {
        public static readonly CandidateStatus[] ManagerVisibleStatuses =
        [
            CandidateStatus.SentToManager,
            CandidateStatus.InterviewScheduled,
            CandidateStatus.InterviewCompleted,
            CandidateStatus.TechnicalSelected,
            CandidateStatus.TechnicalRejected,
            CandidateStatus.OnHold
        ];

        public static readonly CandidateStatus[] HrScreeningStatuses =
        [
            CandidateStatus.HRScreening,
            CandidateStatus.Shortlisted,
            CandidateStatus.OnHold
        ];

        public static bool IsVisibleToManager(CandidateStatus status) =>
            ManagerVisibleStatuses.Contains(status);

        public static bool CanAccessCandidate(string? role, CandidateStatus status) =>
            role switch
            {
                Roles.Manager => IsVisibleToManager(status),
                _ => true
            };
    }
}
