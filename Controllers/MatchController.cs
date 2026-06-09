using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetByJob(int jobId)
        {
            return Ok(await _matchService.GetMatchesForJobAsync(jobId));
        }

        [HttpGet("job/{jobId}/candidate/{candidateId}")]
        public async Task<IActionResult> GetMatch(int jobId, int candidateId)
        {
            var match = await _matchService.GetMatchAsync(candidateId, jobId);
            if (match == null)
                return NotFound();

            return Ok(match);
        }
    }
}
