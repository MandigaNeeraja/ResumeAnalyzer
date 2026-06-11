using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Data;
using ResumeAnalyzer.DTOs.Interview;
using ResumeAnalyzer.Interfaces;
using System.Security.Claims;

namespace ResumeAnalyzer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/interviews")]
    public class InterviewController : ControllerBase
    {
        private readonly IInterviewService _interviewService;
        private readonly AppDbContext _context;

        public InterviewController(
            IInterviewService interviewService,
            AppDbContext context)
        {
            _interviewService = interviewService;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> Create(CreateInterviewDto dto)
        {
            var userId = await GetCurrentUserIdAsync();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _interviewService.CreateInterviewAsync(dto, userId.Value);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _interviewService.GetInterviewsAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var interview = await _interviewService.GetInterviewByIdAsync(id);
            if (interview == null)
                return NotFound();

            return Ok(interview);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> Update(int id, UpdateInterviewDto dto)
        {
            try
            {
                var interview = await _interviewService.UpdateInterviewAsync(id, dto);
                if (interview == null)
                    return NotFound();

                return Ok(interview);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = Roles.AdminOrHR)]
        public async Task<IActionResult> Complete(int id)
        {
            var interview = await _interviewService.CompleteInterviewAsync(id);
            if (interview == null)
                return NotFound();

            return Ok(interview);
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
