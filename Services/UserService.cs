using ResumeAnalyzer.Constants;
using ResumeAnalyzer.DTOs.User;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Interfaces;
using ResumeAnalyzer.Models;
using ResumeAnalyzer.Repositories;

namespace ResumeAnalyzer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private static readonly HashSet<string> ValidRoles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                Roles.Admin,
                Roles.HR,
                Roles.Manager
            };

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserResponseDto>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            if (!ValidRoles.Contains(dto.Role))
                throw new InvalidOperationException(
                    "Role must be Admin, HR, or Manager");

            var existing = await _userRepository.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException("User with this email already exists");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Role = dto.Role,
                Organization = dto.Organization
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            if (!ValidRoles.Contains(dto.Role))
                throw new InvalidOperationException(
                    "Role must be Admin, HR, or Manager");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return null;

            user.Name = dto.Name;
            user.Role = dto.Role;
            user.Organization = dto.Organization;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return false;

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        private static UserResponseDto MapToDto(User user) =>
            new()
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Organization = user.Organization
            };
    }
}
