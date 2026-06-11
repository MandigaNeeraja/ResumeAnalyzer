using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.DTOs.Job;
using ResumeAnalyzer.Interfaces;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/jobs")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Create(CreateJobDto dto)
        {
            var result = await _jobService.CreateJobAsync(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? department,
            [FromQuery] string? search)
        {
            var filter = new JobFilterDto
            {
                Status = status,
                Department = department,
                Search = search
            };
            return Ok(await _jobService.GetJobsAsync(filter));
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            return Ok(await _jobService.GetDepartmentsAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpGet("{id}/candidates/summary")]
        public async Task<IActionResult> GetCandidateSummary(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound();

            return Ok(await _jobService.GetCandidateSummaryAsync(id));
        }

        [HttpGet("{id}/activity")]
        public async Task<IActionResult> GetActivity(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
                return NotFound();

            return Ok(await _jobService.GetActivityLogsAsync(id));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Update(int id, UpdateJobDto dto)
        {
            var job = await _jobService.UpdateJobAsync(id, dto);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpPut("{id}/close")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Close(int id)
        {
            var job = await _jobService.CloseJobAsync(id);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpPut("{id}/reopen")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Reopen(int id)
        {
            var job = await _jobService.ReopenJobAsync(id);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpPut("{id}/hold")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Hold(int id)
        {
            var job = await _jobService.HoldJobAsync(id);
            if (job == null)
                return NotFound();

            return Ok(job);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _jobService.DeleteJobAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
