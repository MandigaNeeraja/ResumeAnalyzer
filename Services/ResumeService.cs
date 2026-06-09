using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Resume;
using ResumeAnalyzer.Enums;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Services
{
    public class ResumeService : IResumeService
    {
        private readonly AppDbContext _context;
        private readonly PdfParserService _pdfParser;
        private readonly DocxParserService _docxParser;
        private readonly CandidateParserService _candidateParser;
        private readonly SkillExtractionService _skillExtractor;
        private readonly IMatchService _matchService;

        public ResumeService(
            AppDbContext context,
            PdfParserService pdfParser,
            DocxParserService docxParser,
            CandidateParserService candidateParser,
            SkillExtractionService skillExtractor,
            IMatchService matchService)
        {
            _context = context;
            _pdfParser = pdfParser;
            _docxParser = docxParser;
            _candidateParser = candidateParser;
            _skillExtractor = skillExtractor;
            _matchService = matchService;
        }

        public async Task<ResumeResponseDto> UploadResumeAsync(ResumeUploadDto dto)
        {
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() +
                Path.GetExtension(dto.ResumeFile.FileName);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.ResumeFile.CopyToAsync(stream);
            }

            string extractedText = "";
            var extension = Path.GetExtension(filePath).ToLower();
            var parseStatus = "Parsed";
            string? parseError = null;

            try
            {
                if (extension == ".pdf")
                    extractedText = _pdfParser.ExtractText(filePath);
                else if (extension == ".docx")
                    extractedText = _docxParser.ExtractText(filePath);
                else
                {
                    parseStatus = "Failed";
                    parseError = "Unsupported file format. Only PDF and DOCX are supported.";
                }
            }
            catch (Exception ex)
            {
                parseStatus = "Failed";
                parseError = ex.Message;
            }

            var skills = string.IsNullOrWhiteSpace(extractedText)
                ? new List<string>()
                : _skillExtractor.ExtractSkills(extractedText);

            var candidate = new Candidate
            {
                FullName = _candidateParser.ExtractName(
                    extractedText,
                    dto.ResumeFile.FileName),

                Email = parseStatus == "Parsed"
                    ? _candidateParser.ExtractEmail(extractedText)
                    : "",

                Phone = parseStatus == "Parsed"
                    ? _candidateParser.ExtractPhone(extractedText)
                    : "",

                ExperienceYears = parseStatus == "Parsed"
                    ? _candidateParser.ExtractExperienceYears(extractedText)
                    : null,

                Status = parseStatus == "Parsed"
                    ? CandidateStatus.Parsed
                    : CandidateStatus.Uploaded,

                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(candidate.Email))
                candidate.Email = null;

            _context.Candidates.Add(candidate);
            await _context.SaveChangesAsync();

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

                _context.CandidateSkills.Add(new CandidateSkill
                {
                    CandidateId = candidate.CandidateId,
                    SkillId = skill.SkillId,
                    Source = "Resume"
                });
            }

            await _context.SaveChangesAsync();

            var resume = new Resume
            {
                CandidateId = candidate.CandidateId,
                FileName = dto.ResumeFile.FileName,
                FilePath = filePath,
                ExtractedText = extractedText,
                ParseStatus = parseStatus,
                ParseErrorMessage = parseError,
                ParsedAt = DateTime.UtcNow
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            var response = new ResumeResponseDto
            {
                ResumeId = resume.ResumeId,
                CandidateId = candidate.CandidateId,
                JobId = dto.JobId > 0 ? dto.JobId : null,
                Name = candidate.FullName,
                Email = candidate.Email ?? "",
                Phone = candidate.Phone,
                Skills = skills,
                ParseStatus = resume.ParseStatus,
                FileName = resume.FileName,
                UploadedOn = resume.ParsedAt
            };

            if (dto.JobId > 0 && parseStatus == "Parsed")
            {
                var match = await _matchService
                    .MatchCandidateToJobAsync(
                        candidate.CandidateId,
                        dto.JobId);

                response.MatchScore = match.MatchScore;
                response.SkillsMatched = match.SkillsMatched;
                response.MatchStatus = match.Status;
            }

            return response;
        }

        public async Task<List<ResumeResponseDto>> GetResumesByJobAsync(int jobId)
        {
            var candidateIds = await _context.CandidateJobMatches
                .Where(m => m.JobId == jobId)
                .Select(m => m.CandidateId)
                .ToListAsync();

            var resumes = await _context.Resumes
                .Include(r => r.Candidate)
                .Where(r => r.CandidateId != null &&
                    candidateIds.Contains(r.CandidateId.Value))
                .OrderByDescending(r => r.ParsedAt)
                .ToListAsync();

            var result = new List<ResumeResponseDto>();

            foreach (var resume in resumes)
            {
                var match = await _matchService.GetMatchAsync(
                    resume.CandidateId!.Value,
                    jobId);

                result.Add(new ResumeResponseDto
                {
                    ResumeId = resume.ResumeId,
                    CandidateId = resume.CandidateId!.Value,
                    JobId = jobId,
                    Name = resume.Candidate!.FullName,
                    Email = resume.Candidate.Email ?? "",
                    Phone = resume.Candidate.Phone,
                    Skills = await _context.CandidateSkills
                        .Where(cs => cs.CandidateId == resume.CandidateId)
                        .Include(cs => cs.Skill)
                        .Select(cs => cs.Skill.SkillName)
                        .ToListAsync(),
                    ParseStatus = resume.ParseStatus,
                    FileName = resume.FileName,
                    UploadedOn = resume.ParsedAt,
                    MatchScore = match?.MatchScore,
                    SkillsMatched = match?.SkillsMatched ?? new(),
                    MatchStatus = match?.Status
                });
            }

            return result;
        }

        public async Task<bool> DeleteResumeAsync(int resumeId)
        {
            var resume = await _context.Resumes
                .Include(r => r.Candidate)
                .FirstOrDefaultAsync(r => r.ResumeId == resumeId);

            if (resume == null)
                return false;

            if (File.Exists(resume.FilePath))
                File.Delete(resume.FilePath);

            if (resume.CandidateId.HasValue)
            {
                var candidateSkills = await _context.CandidateSkills
                    .Where(cs => cs.CandidateId == resume.CandidateId)
                    .ToListAsync();

                var matches = await _context.CandidateJobMatches
                    .Where(m => m.CandidateId == resume.CandidateId)
                    .ToListAsync();

                _context.CandidateSkills.RemoveRange(candidateSkills);
                _context.CandidateJobMatches.RemoveRange(matches);

                if (resume.Candidate != null)
                    _context.Candidates.Remove(resume.Candidate);
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
