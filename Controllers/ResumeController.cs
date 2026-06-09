using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs.Resume;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;

        public ResumeController(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] ResumeUploadDto dto)
        {
            if (dto.ResumeFile == null || dto.ResumeFile.Length == 0)
                return BadRequest("Resume file is required");

            var result = await _resumeService.UploadResumeAsync(dto);
            return Ok(result);
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetByJob(int jobId)
        {
            return Ok(await _resumeService.GetResumesByJobAsync(jobId));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _resumeService.DeleteResumeAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
