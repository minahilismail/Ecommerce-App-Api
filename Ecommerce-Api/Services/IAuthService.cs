using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;

namespace Ecommerce_Api.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<string?> LoginAsync(UserDto request);

    }
}
