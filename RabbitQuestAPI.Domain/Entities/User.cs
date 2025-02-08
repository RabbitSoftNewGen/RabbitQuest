using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public List<UserQuizStatus> UserQuizStatuses { get; set; }

        [Url]
        public string? AvatarURL { get; set; }

        public string? RefreshToken { get; set; } 
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
