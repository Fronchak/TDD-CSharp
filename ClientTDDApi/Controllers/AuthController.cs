using ClientTDDApi.DTOs.Auth;
using ClientTDDApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClientTDDApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userRegisterDTO)
        {
            await _authService.Register(userRegisterDTO);
            return NoContent();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            TokenDTO tokenDTO = await _authService.Login(loginDTO);
            return Ok(tokenDTO);
        }
    }
}
