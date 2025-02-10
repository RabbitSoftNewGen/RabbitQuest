using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Services;
using RabbitQuestAPI.Domain.Entities;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;

    public UserController(IUserService userService, UserManager<User> userManager)
    {
        _userService = userService;
        _userManager = userManager;
    }

    [HttpPost("{userId}/avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(int userId, IFormFile avatarFile)
    {
        try
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                return BadRequest("Avatar file is required.");
            }

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (currentUserId != userId)
            {
                return Forbid("You can only upload avatar for your own profile.");
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
            return StatusCode(500, "An error occurred while uploading the avatar.");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile()
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userProfile = await _userService.GetUserProfileAsync(userId);

            if (userProfile == null)
            {
                return NotFound("User not found.");
            }

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while retrieving the user profile.");
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
            return StatusCode(500, "An error occurred while retrieving the avatar.");
        }
    }
}