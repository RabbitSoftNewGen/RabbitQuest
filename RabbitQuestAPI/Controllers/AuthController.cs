using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Services;

namespace RabbitQuestAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
             
                var tokens = await _authService.LoginAsync(loginDto);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        

        [HttpPost("register-and-login")]
        public async Task<ActionResult> RegisterAndLogin([FromBody] RegisterDto registerDto)
        {
            try
            {
                
                var tokens = await _authService.RegisterAndLoginAsync(registerDto);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
               
                var tokens = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}