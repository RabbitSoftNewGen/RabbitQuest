using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserService userService,
        UserManager<User> userManager,
        ILogger<UserController> logger)
    {
        _userService = userService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpPost("avatar")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UploadAvatar(IFormFile avatarFile)
    {
        _logger.LogInformation("UploadAvatar called. Auth Status: {IsAuthenticated}, Type: {AuthType}",
            User.Identity?.IsAuthenticated, User.Identity?.AuthenticationType);
        _logger.LogInformation("Claims: {Claims}",
            string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

        try
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                return BadRequest("Avatar file is required.");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("No NameIdentifier claim found in token");
                return Unauthorized("User ID not found in token");
            }

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                _logger.LogWarning("Invalid user ID format in token: {UserId}", userIdClaim);
                return BadRequest("Invalid user ID format");
            }

            string avatarUrl = await _userService.UploadAvatarAsync(currentUserId, avatarFile);
            return Ok(new { AvatarURL = avatarUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avata");
            return StatusCode(500, "An error occurred while uploading the avatar.");
        }
    }

    [HttpGet("profile")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile()
    {
        _logger.LogInformation("GetUserProfile called. Auth Status: {IsAuthenticated}, Type: {AuthType}",
            User.Identity?.IsAuthenticated, User.Identity?.AuthenticationType);
        _logger.LogInformation("Claims: {Claims}",
            string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("No NameIdentifier claim found in token");
                return Unauthorized("User ID not found in token");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("Invalid user ID format in token: {UserId}", userIdClaim);
                return BadRequest("Invalid user ID format");
            }

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return NotFound("User not found.");
            }

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, "An error occurred while retrieving the user profile.");
        }
    }

    [HttpGet("avatar")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> GetUserAvatar()
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("No NameIdentifier claim found in token");
                return Unauthorized("User ID not found in token");
            }

            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                _logger.LogWarning("Invalid user ID format in token: {UserId}", userIdClaim);
                return BadRequest("Invalid user ID format");
            }

            var avatarUrl = await _userService.GetUserAvatarAsync(currentUserId);
            if (string.IsNullOrEmpty(avatarUrl))
            {
                return NotFound("Avatar not found.");
            }

            return Ok(new { AvatarURL = avatarUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving avatar");
            return StatusCode(500, "An error occurred while retrieving the avatar.");
        }
    }


   
}