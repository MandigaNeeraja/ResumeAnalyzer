using Microsoft.AspNetCore.Http;

namespace ResumeAnalyzer.DTOs.Resume
{
    public class ResumeUploadDto
    {
        public int JobId { get; set; }

        public IFormFile ResumeFile { get; set; } = null!;
    }
}