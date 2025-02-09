using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http; // For configuration

namespace RabbitQuestAPI.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _storageBasePath; // Base path for storage

        public UserService(ApplicationDbContext context, IConfiguration configuration) 
        {
            _context = context;
            _storageBasePath = configuration["Storage:BasePath"]; 

            
            if (!_storageBasePath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                _storageBasePath += Path.DirectorySeparatorChar;
            }

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

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(_storageBasePath, "uploads", "avatars", fileName);


            Directory.CreateDirectory(Path.GetDirectoryName(filePath));


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var avatarUrl = $"/uploads/avatars/{fileName}"; // Or however you construct the URL
            user.AvatarURL = avatarUrl;
            await _context.SaveChangesAsync();
            return avatarUrl;
        }


        public async Task<string> GetUserAvatarAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            return user?.AvatarURL;
        }
    }
}