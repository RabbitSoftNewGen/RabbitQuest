using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitQuestAPI.Application.Services;

namespace RabbitQuestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("{userId}/avatar")] 
        public async Task<IActionResult> UploadAvatar(int userId, IFormFile avatarFile)
        {
            try
            {
                if (avatarFile == null || avatarFile.Length == 0)
                {
                    return BadRequest("Avatar file is required.");
                }


                string avatarUrl = await _userService.UploadAvatarAsync(userId, avatarFile);

                return Ok(new { AvatarURL = avatarUrl });  

            }
            catch (ArgumentException ex) 
            {
                return NotFound(ex.Message); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); 
            }
        }

        [HttpGet("{userId}/avatar")]
        public async Task<ActionResult> GetUserAvatar(int userId)
        {
            try
            {
                var avatarUrl = await _userService.GetUserAvatarAsync(userId);

                if (string.IsNullOrEmpty(avatarUrl))
                {
                    return NotFound("Avatar not found.");
                }

                return Ok(new { AvatarURL = avatarUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
