using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Job;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Repositories;
using System.Security.Claims;

namespace ResumeAnalyzer.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly AppDbContext _context;
        private readonly SkillExtractionService _skillExtractor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobService(
            IJobRepository jobRepository,
            AppDbContext context,
            SkillExtractionService skillExtractor,
            IHttpContextAccessor httpContextAccessor)
        {
            _jobRepository = jobRepository;
            _context = context;
            _skillExtractor = skillExtractor;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<JobResponseDto> CreateJobAsync(CreateJobDto dto)
        {
            var userId = await GetCurrentUserIdAsync();

            var job = new Job
            {
                Title = dto.JobTitle,
                Designation = dto.Designation,
                Department = dto.Department,
                Location = dto.Location,
                EmploymentType = dto.EmploymentType,
                Experience = dto.ExperienceRequired,
                Description = dto.Description,
                Status = JobStatus.Open,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            var skills = GetUniqueSkills(dto.RequiredSkills, dto.Description);
            await SaveJobSkillsAsync(job.JobId, skills);

            await JobActivityHelper.LogAsync(
                _context, job.JobId, JobActivityType.JobCreated,
                $"Job \"{job.Title}\" created in {job.Department}.",
                userId);

            return (await MapJobAsync(job.JobId))!;
        }

        public async Task<List<JobResponseDto>> GetJobsAsync(JobFilterDto? filter = null)
        {
            var query = _context.Jobs
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.Creator)
                .AsQueryable();

            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Status) &&
                    Enum.TryParse<JobStatus>(filter.Status, true, out var status))
                {
                    query = query.Where(j => j.Status == status);
                }

                if (!string.IsNullOrWhiteSpace(filter.Department))
                {
                    query = query.Where(j => j.Department == filter.Department);
                }

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    var term = filter.Search.Trim();
                    query = query.Where(j =>
                        j.Title.Contains(term) ||
                        j.Department.Contains(term) ||
                        j.Designation.Contains(term));
                }
            }

            var jobs = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var result = new List<JobResponseDto>();
            foreach (var job in jobs)
                result.Add(await MapJobFromEntityAsync(job));

            return result;
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(int id) =>
            await MapJobAsync(id);

        public async Task<JobResponseDto?> UpdateJobAsync(int id, UpdateJobDto dto)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                return null;

            job.Title = dto.JobTitle;
            job.Designation = dto.Designation;
            job.Department = dto.Department;
            job.Location = dto.Location;
            job.EmploymentType = dto.EmploymentType;
            job.Experience = dto.ExperienceRequired;
            job.Description = dto.Description;
            job.UpdatedAt = DateTime.UtcNow;

            var existingSkills = await _context.JobSkills
                .Where(js => js.JobId == id)
                .ToListAsync();

            _context.JobSkills.RemoveRange(existingSkills);

            var skills = GetUniqueSkills(dto.RequiredSkills, dto.Description);
            await SaveJobSkillsAsync(id, skills);

            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();
            await JobActivityHelper.LogAsync(
                _context, id, JobActivityType.JobUpdated,
                $"Job \"{job.Title}\" updated.", userId);

            return await MapJobAsync(id);
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                return false;

            var activityLogs = await _context.JobActivityLogs
                .Where(a => a.JobId == id)
                .ToListAsync();

            var interviews = await _context.Interviews
                .Where(i => i.JobId == id)
                .ToListAsync();

            var jobSkills = await _context.JobSkills
                .Where(js => js.JobId == id)
                .ToListAsync();

            var matches = await _context.CandidateJobMatches
                .Where(m => m.JobId == id)
                .ToListAsync();

            _context.JobActivityLogs.RemoveRange(activityLogs);
            _context.Interviews.RemoveRange(interviews);
            _context.JobSkills.RemoveRange(jobSkills);
            _context.CandidateJobMatches.RemoveRange(matches);
            _jobRepository.Remove(job);
            await _jobRepository.SaveChangesAsync();

            return true;
        }

        public async Task<JobResponseDto?> CloseJobAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                return null;

            job.Status = JobStatus.Closed;
            job.UpdatedAt = DateTime.UtcNow;
            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();
            await JobActivityHelper.LogAsync(
                _context, id, JobActivityType.JobClosed,
                $"Job \"{job.Title}\" closed.", userId);

            return await MapJobAsync(id);
        }

        public async Task<JobResponseDto?> ReopenJobAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                return null;

            job.Status = JobStatus.Open;
            job.UpdatedAt = DateTime.UtcNow;
            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();
            await JobActivityHelper.LogAsync(
                _context, id, JobActivityType.JobReopened,
                $"Job \"{job.Title}\" reopened.", userId);

            return await MapJobAsync(id);
        }

        public async Task<JobResponseDto?> HoldJobAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
                return null;

            job.Status = JobStatus.OnHold;
            job.UpdatedAt = DateTime.UtcNow;
            _jobRepository.Update(job);
            await _jobRepository.SaveChangesAsync();

            var userId = await GetCurrentUserIdAsync();
            await JobActivityHelper.LogAsync(
                _context, id, JobActivityType.JobPutOnHold,
                $"Job \"{job.Title}\" put on hold.", userId);

            return await MapJobAsync(id);
        }

        public async Task<List<string>> GetDepartmentsAsync()
        {
            var fromDb = await _context.Jobs
                .Where(j => !string.IsNullOrEmpty(j.Department))
                .Select(j => j.Department)
                .Distinct()
                .ToListAsync();

            return Departments.All
                .Concat(fromDb)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList();
        }

        public async Task<JobCandidateSummaryDto> GetCandidateSummaryAsync(int jobId)
        {
            var candidates = await _context.Candidates
                .Where(c => c.JobId == jobId)
                .ToListAsync();

            return new JobCandidateSummaryDto
            {
                All = candidates.Count,
                Shortlisted = candidates.Count(c => c.Status == CandidateStatus.Shortlisted),
                OnHold = candidates.Count(c => c.Status == CandidateStatus.OnHold),
                Rejected = candidates.Count(c =>
                    c.Status == CandidateStatus.Rejected ||
                    c.Status == CandidateStatus.TechnicalRejected),
                SentToManager = candidates.Count(c => c.Status == CandidateStatus.SentToManager),
                Selected = candidates.Count(c =>
                    c.Status == CandidateStatus.TechnicalSelected ||
                    c.Status == CandidateStatus.Hired)
            };
        }

        public async Task<List<JobActivityLogDto>> GetActivityLogsAsync(int jobId)
        {
            return await _context.JobActivityLogs
                .Where(a => a.JobId == jobId)
                .Include(a => a.Performer)
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .Select(a => new JobActivityLogDto
                {
                    Id = a.JobActivityLogId,
                    JobId = a.JobId,
                    ActivityType = JobActivityHelper.FormatActivityType(a.ActivityType),
                    Description = a.Description,
                    PerformedByName = a.Performer != null ? a.Performer.Name : null,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
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

        private List<string> GetUniqueSkills(
            List<string> explicitSkills,
            string description)
        {
            var extracted = _skillExtractor.ExtractSkills(description);
            return explicitSkills
                .Concat(extracted)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task SaveJobSkillsAsync(int jobId, List<string> skills)
        {
            foreach (var skillName in skills)
            {
                var skill = await _context.Skills
                    .FirstOrDefaultAsync(x => x.SkillName == skillName);

                if (skill == null)
                {
                    skill = new Skill { SkillName = skillName };
                    _context.Skills.Add(skill);
                    await _context.SaveChangesAsync();
                }

                _context.JobSkills.Add(new JobSkill
                {
                    JobId = jobId,
                    SkillId = skill.SkillId
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<JobResponseDto?> MapJobAsync(int jobId)
        {
            var job = await _jobRepository.GetWithSkillsAsync(jobId);
            return job == null ? null : await MapJobFromEntityAsync(job);
        }

        private async Task<JobResponseDto> MapJobFromEntityAsync(Job job)
        {
            var resumeCount = await _context.CandidateJobMatches
                .CountAsync(m => m.JobId == job.JobId);

            var matchCount = await _context.CandidateJobMatches
                .CountAsync(m => m.JobId == job.JobId && m.ATSScore >= 60);

            return new JobResponseDto
            {
                JobId = job.JobId,
                JobTitle = job.Title,
                Designation = job.Designation,
                Department = job.Department,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                ExperienceRequired = job.Experience,
                RequiredSkills = job.JobSkills
                    .Select(js => js.Skill.SkillName)
                    .ToList(),
                Description = job.Description,
                CreatedBy = job.CreatedBy,
                CreatedByName = job.Creator?.Name,
                CreatedDate = job.CreatedAt,
                UpdatedDate = job.UpdatedAt,
                Status = job.Status.ToString(),
                ResumeCount = resumeCount,
                MatchCount = matchCount
            };
        }
    }
}
