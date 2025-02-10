using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Domain.Entities;

namespace RabbitQuestAPI.Infrastructure.Repositories
{
    public class QuizRepository : RepositoryBase<Quiz>, IQuizRepository
    {
        public QuizRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
