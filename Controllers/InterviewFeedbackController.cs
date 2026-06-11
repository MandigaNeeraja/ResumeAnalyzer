using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.InterviewFeedback;
using ResumeAnalyzer.Interfaces;
using System.Security.Claims;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/interview-feedback")]
    public class InterviewFeedbackController : ControllerBase
    {
        private readonly IInterviewFeedbackService _feedbackService;
        private readonly AppDbContext _context;

        public InterviewFeedbackController(
            IInterviewFeedbackService feedbackService,
            AppDbContext context)
        {
            _feedbackService = feedbackService;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminOrManager)]
        public async Task<IActionResult> Create(CreateInterviewFeedbackDto dto)
        {
            var managerId = await GetCurrentUserIdAsync();
            if (managerId == null)
                return Unauthorized();

            try
            {
                var result = await _feedbackService.CreateFeedbackAsync(dto, managerId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{candidateId}")]
        public async Task<IActionResult> GetByCandidate(int candidateId)
        {
            return Ok(await _feedbackService.GetFeedbackByCandidateIdAsync(candidateId));
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            return user?.UserId;
        }
    }
}
