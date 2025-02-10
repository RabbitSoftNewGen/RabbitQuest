using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Application.Services;
using RabbitQuestAPI.Domain.Entities;
using RabbitQuestAPI.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly string _storageBasePath;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly IQuizService _quizService;

    public UserService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IUserRepository userRepository,
        UserManager<User> userManager,
        IQuizService quizService)
    {
        _context = context;
        _storageBasePath = configuration["Storage:BasePath"];
        _userRepository = userRepository;
        _userManager = userManager;
        _quizService = quizService;

        // Ensure the base path ends with a directory separator
        if (!_storageBasePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            _storageBasePath += Path.DirectorySeparatorChar;
        }

        // Ensure the storage directory exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    public async Task<string> UploadAvatarAsync(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty.");
        }

        // Generate a unique file name
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(_storageBasePath, "uploads", "avatars", fileName);

        // Ensure the directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // Save the file to the local file system
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update the user's avatar URL in the database
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found.");
        }

        var avatarUrl = $"/uploads/avatars/{fileName}"; // Construct the URL
        user.AvatarURL = avatarUrl;
        await _context.SaveChangesAsync();

        return avatarUrl;
    }

    public async Task<string> GetUserAvatarAsync(int userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.AvatarURL;
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        var users = await _userRepository.GetAllAsync();
        return users.FirstOrDefault(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var users = await _userRepository.GetAllAsync();
        return users.Any(u => u.Email == email);
    }

    public async Task<User> GetByRefreshTokenAsync(string refreshToken)
    {
        var users = await _userRepository.GetAllAsync();
        return users.FirstOrDefault(u => u.RefreshToken == refreshToken);
    }

    public async Task UpdateUserTokensAsync(User user, string refreshToken, DateTime expiryTime)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiryTime;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetUserProfileAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
        {
            return null;
        }

        var userQuizStatuses = await _quizService.GetUserQuizStatusesAsync(userId);

        var completedQuizzes = userQuizStatuses
            .Where(uqs => uqs.QuizStatus == QuizStatus.Completed)
            .Select(uqs => new QuizDto
            {
                Id = uqs.QuizId,
                Title = uqs.Quiz.Title,
                CompletedAt = uqs.CompletedAt
            })
            .ToList();

        var notCompletedQuizzes = userQuizStatuses
            .Where(uqs => uqs.QuizStatus == QuizStatus.Clicked)
            .Select(uqs => new QuizDto
            {
                Id = uqs.QuizId,
                Title = uqs.Quiz.Title
            })
            .ToList();

        var createdQuizzes = userQuizStatuses
            .Where(uqs => uqs.QuizStatus == QuizStatus.Created)
            .Select(uqs => new QuizDto
            {
                Id = uqs.QuizId,
                Title = uqs.Quiz.Title,
                Description = uqs.Quiz.Description,
                Category = new CategoryDto
                {
                    Id = uqs.Quiz.Category.Id,
                    Name = uqs.Quiz.Category.Name
                }
            })
            .ToList();

        return new UserProfileDto
        {
            Username = user.UserName,
            AvatarURL = user.AvatarURL,
            CompletedQuizzes = completedQuizzes,
            NotCompletedQuizzes = notCompletedQuizzes,
            CreatedQuizzes = createdQuizzes

        };
    }

    public async Task AddUserAsync(User user)
    {
        var result = await _userManager.CreateAsync(user, user.PasswordHash);

        if (!result.Succeeded)
        {
            throw new Exception("User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}