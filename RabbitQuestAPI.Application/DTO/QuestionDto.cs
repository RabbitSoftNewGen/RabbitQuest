using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RabbitQuestAPI.Application.DTO
{
    public class QuestionDto
    {
        [Required]
        public string Title { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0.")]
        public int Points { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Time limit must be greater than 0.")] 
        public int TimeLimit { get; set; }

        [Required]
        public List<string> CorrectAnswers { get; set; }

        public string? Image { get; set; }

        public string? Video { get; set; }

        public List<string>? Answers { get; set; }
    }
}