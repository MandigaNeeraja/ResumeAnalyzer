using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Resume;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Enums;

namespace ResumeAnalyzer.Services
{
    public class ResumeService : IResumeService
    {
    //    private readonly AppDbContext _context;
    //    private readonly PdfParserService _pdfParser;
    //    private readonly DocxParserService _docxParser;
    //    private readonly CandidateParserService _candidateParser;
    //    private readonly SkillExtractionService _skillExtractor;
    //    private readonly IOpenAIService _openAIService;

    //    public ResumeService(
    //AppDbContext context,
    //PdfParserService pdfParser,
    //DocxParserService docxParser,
    //CandidateParserService candidateParser,
    //SkillExtractionService skillExtractor,
    //IOpenAIService openAIService
    //        )
    private readonly AppDbContext _context;
    private readonly PdfParserService _pdfParser;
    private readonly DocxParserService _docxParser;
    private readonly CandidateParserService _candidateParser;
    private readonly SkillExtractionService _skillExtractor;

    public ResumeService(
        AppDbContext context,
        PdfParserService pdfParser,
        DocxParserService docxParser,
        CandidateParserService candidateParser,
        SkillExtractionService skillExtractor)
    {
        _context = context;
        _pdfParser = pdfParser;
        _docxParser = docxParser;
        _candidateParser = candidateParser;
        _skillExtractor = skillExtractor;
    }


        public async Task<ResumeResponseDto>
    UploadResumeAsync(
    ResumeUploadDto dto)
        {
            var uploadsFolder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(
                    uploadsFolder);
            }

            var fileName =
                Guid.NewGuid() +
                Path.GetExtension(
                    dto.ResumeFile.FileName);

            var filePath =
                Path.Combine(
                    uploadsFolder,
                    fileName);

            using (var stream =
                new FileStream(
                    filePath,
                    FileMode.Create))
            {
                await dto.ResumeFile.CopyToAsync(stream);
            }

            string extractedText = "";

            var extension =
                Path.GetExtension(filePath)
                    .ToLower();

            if (extension == ".pdf")
            {
                extractedText =
                    _pdfParser.ExtractText(filePath);
            }
            else if (extension == ".docx")
            {
                extractedText =
                    _docxParser.ExtractText(filePath);
            }

            var skills =
                _skillExtractor
                .ExtractSkills(extractedText);

            var candidate = new Candidate
            {
                FullName =
                    _candidateParser
                    .ExtractName(extractedText),

                Email =
                    _candidateParser
                    .ExtractEmail(extractedText),

                Phone =
                    _candidateParser
                    .ExtractPhone(extractedText),

                Status =
                    CandidateStatus.Parsed
            };

            _context.Candidates.Add(candidate);

            await _context.SaveChangesAsync();

            foreach (var skillName in skills)
            {
                var skill =
                    _context.Skills
                    .FirstOrDefault(x =>
                        x.SkillName == skillName);

                if (skill == null)
                {
                    skill = new Skill
                    {
                        SkillName = skillName
                    };

                    _context.Skills.Add(skill);

                    await _context.SaveChangesAsync();
                }

                _context.CandidateSkills.Add(
                    new CandidateSkill
                    {
                        CandidateId =
                            candidate.CandidateId,

                        SkillId =
                            skill.SkillId,

                        Source = "Resume"
                    });
            }

            await _context.SaveChangesAsync();

            var resume = new Resume
            {
                CandidateId =
                    candidate.CandidateId,

                FileName =
                    dto.ResumeFile.FileName,

                FilePath =
                    filePath,

                ExtractedText =
                    extractedText,

                ParseStatus =
                    "Parsed",

                ParsedAt =
                    DateTime.UtcNow
            };

            _context.Resumes.Add(resume);

            await _context.SaveChangesAsync();

            return new ResumeResponseDto
            {
                ResumeId = resume.ResumeId,

                CandidateId =
                    candidate.CandidateId,

                Name =
                    candidate.FullName,

                Email =
                    candidate.Email ?? "",

                Phone =
                    candidate.Phone,

                Skills =
                    skills,

                ParseStatus =
                    resume.ParseStatus
            };
        }
    }
}