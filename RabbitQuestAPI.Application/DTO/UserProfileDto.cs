using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.DTO
{
    public class UserProfileDto : UserDto
    {
        public List<QuizDto> CompletedQuizzes { get; set; } = new();
        public List<QuizDto> NotCompletedQuizzes { get; set; } = new();

        public double AverageRating { get; set; }

        public List<QuizDto> CreatedQuizzes { get; set; } = new();
    }
}
