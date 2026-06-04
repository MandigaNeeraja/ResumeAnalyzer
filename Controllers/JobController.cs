using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.DTOs.Job;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost]
        public async Task<IActionResult>
            Create(CreateJobDto dto)
        {
            var result =
                await _jobService
                .CreateJobAsync(dto);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult>
            GetAll()
        {
            return Ok(
                await _jobService
                .GetJobsAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetById(int id)
        {
            var job =
                await _jobService
                .GetJobByIdAsync(id);

            if (job == null)
                return NotFound();

            return Ok(job);
        }
    }
}