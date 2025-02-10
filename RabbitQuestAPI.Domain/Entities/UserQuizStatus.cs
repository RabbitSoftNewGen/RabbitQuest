using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Domain.Entities
{
    public enum QuizStatus { Clicked, Completed, Created }
    public class UserQuizStatus
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }

        [Required]
        public QuizStatus QuizStatus { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
