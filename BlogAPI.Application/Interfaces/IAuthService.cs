using BlogAPI.Application.DTOs;
using BlogAPI.Domain.Abstractions;

namespace BlogAPI.Application.Interfaces
{
    public interface IAuthService 
    {
        Task<Result<string>> RegisterAsync(RegisterDto registerDto);
        Task<Result<string>> LoginAsync(LoginDto loginDto);
    }
}
