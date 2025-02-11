using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Application.DTO
{
    public class CreateQuizDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string Category { get; set; }

        public List<QuestionDto> Questions { get; set; }


    }
}
