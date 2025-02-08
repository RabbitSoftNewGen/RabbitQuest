using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Domain.Entities
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0.")]
        public int Points { get; set; }

        public int QuizId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Time limit must be greater than 0.")]
        public int TimeLimit { get; set; }

        [Required]
        public string CorrectVariant { get; set; }

        public string? Image { get; set; }

        public string? Video { get; set; }

        public List<string>? Variants { get; set; }

        public Quiz Quiz { get; set; }
    }
}
