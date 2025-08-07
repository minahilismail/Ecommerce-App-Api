using Ecommerce_Api.Models;
using Ecommerce_Api.Models.Dto;
using Ecommerce_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(RegisterRequestDto request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user == null)
            {
                return BadRequest("User already exists or invalid data.");
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
        {
            var token = await _authService.LoginAsync(request);
            if (token is null)
            {
                return BadRequest("Invalid username, email or password.");
            }
            return Ok(token);
        }

        
    }
}