using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.DTO
{
    public class GetQuizDto // quiz dto with questions
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public double Rating { get; set; }
        public CategoryDto Category { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<QuestionDto> Questions { get; set; } // Add this property for questions
    }

}
