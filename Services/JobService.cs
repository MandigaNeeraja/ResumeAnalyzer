using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Job;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Services
{
    public class JobService : IJobService
    {
        private readonly AppDbContext _context;
        private readonly SkillExtractionService _skillExtractor;

        public JobService(
            AppDbContext context,
            SkillExtractionService skillExtractor)
        {
            _context = context;
            _skillExtractor = skillExtractor;
        }

        public async Task<JobResponseDto> CreateJobAsync(CreateJobDto dto)
        {
            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description,
                Experience = dto.Experience,
                CreatedAt = DateTime.UtcNow
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            var skills = GetUniqueSkills(dto.Skills, dto.Description);
            await SaveJobSkillsAsync(job.JobId, skills);

            var created = await MapJobAsync(job.JobId);
            return created!;
        }

        public async Task<List<JobResponseDto>> GetJobsAsync()
        {
            var jobIds = await _context.Jobs
                .OrderByDescending(j => j.CreatedAt)
                .Select(j => j.JobId)
                .ToListAsync();

            var result = new List<JobResponseDto>();
            foreach (var jobId in jobIds)
            {
                var mapped = await MapJobAsync(jobId);
                if (mapped != null)
                    result.Add(mapped);
            }

            return result;
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(int id)
        {
            return await MapJobAsync(id);
        }

        public async Task<JobResponseDto?> UpdateJobAsync(int id, UpdateJobDto dto)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return null;

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Experience = dto.Experience;

            var existingSkills = await _context.JobSkills
                .Where(js => js.JobId == id)
                .ToListAsync();

            _context.JobSkills.RemoveRange(existingSkills);

            var skills = GetUniqueSkills(dto.Skills, dto.Description);
            await SaveJobSkillsAsync(id, skills);

            await _context.SaveChangesAsync();

            return await MapJobAsync(id);
        }

        public async Task<bool> DeleteJobAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return false;

            var jobSkills = await _context.JobSkills
                .Where(js => js.JobId == id)
                .ToListAsync();

            var matches = await _context.CandidateJobMatches
                .Where(m => m.JobId == id)
                .ToListAsync();

            _context.JobSkills.RemoveRange(jobSkills);
            _context.CandidateJobMatches.RemoveRange(matches);
            _context.Jobs.Remove(job);

            await _context.SaveChangesAsync();
            return true;
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
            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                    .ThenInclude(js => js.Skill)
                .FirstOrDefaultAsync(j => j.JobId == jobId);

            if (job == null)
                return null;

            var resumeCount = await _context.CandidateJobMatches
                .CountAsync(m => m.JobId == jobId);

            var matchCount = await _context.CandidateJobMatches
                .CountAsync(m => m.JobId == jobId && m.ATSScore >= 60);

            return new JobResponseDto
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                Experience = job.Experience,
                Skills = job.JobSkills
                    .Select(js => js.Skill.SkillName)
                    .ToList(),
                CreatedAt = job.CreatedAt,
                ResumeCount = resumeCount,
                MatchCount = matchCount
            };
        }
    }
}
