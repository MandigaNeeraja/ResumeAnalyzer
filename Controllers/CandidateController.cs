using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CandidateController : ControllerBase
    {
        private readonly ICandidateService _candidateService;

        public CandidateController(ICandidateService candidateService)
        {
            _candidateService = candidateService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _candidateService.GetCandidatesAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var candidate = await _candidateService.GetCandidateByIdAsync(id);
            if (candidate == null)
                return NotFound();

            return Ok(candidate);
        }

        [HttpGet("{id}/job/{jobId}/match")]
        public async Task<IActionResult> GetMatchForJob(int id, int jobId)
        {
            var match = await _candidateService
                .GetCandidateMatchForJobAsync(id, jobId);

            if (match == null)
                return NotFound();

            return Ok(match);
        }
    }
}
