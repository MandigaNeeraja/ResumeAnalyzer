using ResumeAnalyzer.DTOs.Auth;

namespace ResumeAnalyzer.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);

        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    }
}