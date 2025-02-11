using RabbitQuestAPI.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto); 
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken); 
        Task<bool> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponseDto> RegisterAndLoginAsync(RegisterDto registerDto);

        


    }
}
