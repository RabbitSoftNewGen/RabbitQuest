using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.DTO
{
    public class QuizDto
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }

        public CategoryDto Category { get; set; }
        public DateTime? CompletedAt { get; set; }

    }
}
