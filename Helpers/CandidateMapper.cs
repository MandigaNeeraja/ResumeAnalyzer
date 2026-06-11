using ResumeAnalyzer.DTOs.Candidate;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Helpers
{
    public static class CandidateMapper
    {
        public static CandidateResponseDto ToDto(Candidate candidate) =>
            new()
            {
                CandidateId = candidate.CandidateId,
                JobId = candidate.JobId,
                JobTitle = candidate.Job?.Title,
                FullName = candidate.FullName,
                Email = candidate.Email ?? "",
                Phone = candidate.Phone,
                LinkedIn = candidate.LinkedIn,
                Experience = candidate.ExperienceYears.HasValue
                    ? $"{candidate.ExperienceYears} years"
                    : "N/A",
                Skills = candidate.CandidateSkills
                    .Select(cs => cs.Skill.SkillName)
                    .ToList(),
                ATSScore = candidate.ATSScore,
                Status = candidate.Status.ToString(),
                HRRemarks = candidate.HRRemarks,
                ManagerRemarks = candidate.ManagerRemarks,
                ManagerAvailability = ManagerAvailabilityHelper.Deserialize(candidate.ManagerAvailability),
                CreatedDate = candidate.CreatedAt
            };
    }
}
