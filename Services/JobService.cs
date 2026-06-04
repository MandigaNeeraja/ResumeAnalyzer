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

        public async Task<JobResponseDto>
            CreateJobAsync(CreateJobDto dto)
        {
            var job = new Job
            {
                Title = dto.Title,
                Description = dto.Description
            };

            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();

            var extractedSkills =
                _skillExtractor
                .ExtractSkills(dto.Description);

            foreach (var skillName in extractedSkills)
            {
                var skill =
                    await _context.Skills
                    .FirstOrDefaultAsync(
                        x => x.SkillName == skillName);

                if (skill == null)
                {
                    skill = new Skill
                    {
                        SkillName = skillName
                    };

                    _context.Skills.Add(skill);

                    await _context.SaveChangesAsync();
                }

                _context.JobSkills.Add(
                    new JobSkill
                    {
                        JobId = job.JobId,
                        SkillId = skill.SkillId
                    });
            }

            await _context.SaveChangesAsync();

            return new JobResponseDto
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description
            };
        }

        public async Task<List<JobResponseDto>>
            GetJobsAsync()
        {
            return await _context.Jobs
                .Select(x => new JobResponseDto
                {
                    JobId = x.JobId,
                    Title = x.Title,
                    Description = x.Description
                })
                .ToListAsync();
        }

        public async Task<JobResponseDto?>
            GetJobByIdAsync(int id)
        {
            return await _context.Jobs
                .Where(x => x.JobId == id)
                .Select(x => new JobResponseDto
                {
                    JobId = x.JobId,
                    Title = x.Title,
                    Description = x.Description
                })
                .FirstOrDefaultAsync();
        }
    }
}