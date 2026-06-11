using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.DTOs.Candidate;
using ResumeAnalyzer.Interfaces;
using System.Security.Claims;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/candidates")]
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
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            return Ok(await _candidateService.GetCandidatesForRoleAsync(role));
        }

        [HttpGet("hr-screening")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> GetHrScreening()
        {
            return Ok(await _candidateService.GetHrScreeningCandidatesAsync());
        }

        [HttpGet("manager-review")]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> GetManagerReview()
        {
            return Ok(await _candidateService.GetManagerReviewCandidatesAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var candidate = await _candidateService.GetCandidateByIdForRoleAsync(id, role);
            if (candidate == null)
                return NotFound();

            return Ok(candidate);
        }

        [HttpGet("{id}/job/{jobId}/match")]
        public async Task<IActionResult> GetMatchForJob(int id, int jobId)
        {
            var match = await _candidateService.GetCandidateMatchForJobAsync(id, jobId);
            if (match == null)
                return NotFound();

            return Ok(match);
        }

        [HttpPut("{id}/send-to-manager")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> SendToManager(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.SendToManagerAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not eligible to send to manager");

            return Ok(result);
        }

        [HttpPut("{id}/shortlist")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> Shortlist(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.ShortlistCandidateAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not eligible for shortlisting");

            return Ok(result);
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.RejectCandidateAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not eligible for rejection");

            return Ok(result);
        }

        [HttpPut("{id}/hold")]
        public async Task<IActionResult> Hold(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var isManager = role is Roles.Manager or Roles.Admin;
            var isHr = role is Roles.HR or Roles.Admin;

            if (role == Roles.Manager)
            {
                var result = await _candidateService.HoldCandidateAsync(id, dto ?? new CandidateWorkflowDto(), true);
                if (result == null)
                    return BadRequest("Candidate not found or not eligible to put on hold");

                return Ok(result);
            }

            if (isHr)
            {
                var result = await _candidateService.HoldCandidateAsync(id, dto ?? new CandidateWorkflowDto(), false);
                if (result == null)
                    return BadRequest("Candidate not found or not eligible to put on hold");

                return Ok(result);
            }

            return Forbid();
        }

        [HttpPut("{id}/resume-screening")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> ResumeToScreening(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.ResumeToScreeningAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not on hold");

            return Ok(result);
        }

        [HttpPut("{id}/resume-review")]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> ResumeToManagerReview(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.ResumeToManagerReviewAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not on hold");

            return Ok(result);
        }

        [HttpPut("{id}/approve-interview")]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> ApproveInterview(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            try
            {
                var result = await _candidateService.ApproveInterviewAsync(id, dto ?? new CandidateWorkflowDto());
                if (result == null)
                    return BadRequest("Candidate not found or not awaiting manager review");

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/technical-select")]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> TechnicalSelect(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.TechnicalSelectAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not eligible for technical selection");

            return Ok(result);
        }

        [HttpPut("{id}/technical-reject")]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> TechnicalReject(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.TechnicalRejectAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not eligible for technical rejection");

            return Ok(result);
        }

        [HttpPut("{id}/hire")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> Hire(int id, [FromBody] CandidateWorkflowDto? dto)
        {
            var result = await _candidateService.HireAsync(id, dto ?? new CandidateWorkflowDto());
            if (result == null)
                return BadRequest("Candidate not found or not technically selected");

            return Ok(result);
        }
    }
}
