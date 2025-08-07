using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;

namespace Ecommerce_Api.Services
{
    public interface IAuthService
    {
        Task<UserResponseDto?> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    }
}
