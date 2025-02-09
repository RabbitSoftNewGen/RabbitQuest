using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.Services
{
    public interface IUserService
    {
        Task<string?> GetUserAvatarAsync(int userId);

        Task<string> UploadAvatarAsync(int userId, IFormFile file);

    }
}
