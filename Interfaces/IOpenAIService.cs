using ResumeAnalyzer.DTOs.AI;

namespace ResumeAnalyzer.Interfaces
{
    public interface IOpenAIService
    {
        Task<ResumeParseResponse>
            ParseResumeAsync(string resumeText);
    }
}