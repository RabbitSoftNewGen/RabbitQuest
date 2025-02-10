using Microsoft.AspNetCore.Http;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.Services
{
    public interface IUserService
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<User> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateUserTokensAsync(User user, string refreshToken, DateTime expiryTime);
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task AddUserAsync(User user);
        Task<string> UploadAvatarAsync(int userId, IFormFile avatarFile);
        Task<string> GetUserAvatarAsync(int userId);
    }
}
