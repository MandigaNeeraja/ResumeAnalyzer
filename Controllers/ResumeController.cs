using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs.Resume;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;

        public ResumeController(
            IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult>
            Upload(
            [FromForm]
            ResumeUploadDto dto)
        {
            var result =
                await _resumeService
                .UploadResumeAsync(dto);

            return Ok(result);
        }
    }
}