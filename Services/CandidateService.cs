using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Candidate;
using ResumeAnalyzer.DTOs.Match;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly AppDbContext _context;
        private readonly IMatchService _matchService;

        public CandidateService(
            AppDbContext context,
            IMatchService matchService)
        {
            _context = context;
            _matchService = matchService;
        }

        public async Task<List<CandidateResponseDto>> GetCandidatesAsync()
        {
            return await _context.Candidates
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CandidateResponseDto
                {
                    CandidateId = c.CandidateId,
                    Name = c.FullName,
                    Email = c.Email ?? "",
                    Phone = c.Phone,
                    Experience = c.ExperienceYears.HasValue
                        ? $"{c.ExperienceYears} years"
                        : "N/A",
                    Skills = c.CandidateSkills
                        .Select(cs => cs.Skill.SkillName)
                        .ToList(),
                    Status = c.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task<CandidateResponseDto?> GetCandidateByIdAsync(int id)
        {
            return await _context.Candidates
                .Where(c => c.CandidateId == id)
                .Include(c => c.CandidateSkills)
                    .ThenInclude(cs => cs.Skill)
                .Select(c => new CandidateResponseDto
                {
                    CandidateId = c.CandidateId,
                    Name = c.FullName,
                    Email = c.Email ?? "",
                    Phone = c.Phone,
                    Experience = c.ExperienceYears.HasValue
                        ? $"{c.ExperienceYears} years"
                        : "N/A",
                    Skills = c.CandidateSkills
                        .Select(cs => cs.Skill.SkillName)
                        .ToList(),
                    Status = c.Status.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<MatchResponseDto?> GetCandidateMatchForJobAsync(
            int candidateId,
            int jobId)
        {
            return await _matchService.GetMatchAsync(candidateId, jobId);
        }
    }
}
