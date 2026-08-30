using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.User.Application.Dto;
using UserService.User.Application.Interfaces;

namespace UserService.User.Api.Controllers
{
    [Route("auth/")]
    [ApiController]
    public class AuthController : Controller
    {
        IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!await _authService.IsHashPasswordEqual(dto))
                return BadRequest("Неверный логин или пароль");

            var token = await _authService.Login(dto);

            return Ok(token);

        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            _authService.Register(request);

            return NoContent();
        }
    }
}
