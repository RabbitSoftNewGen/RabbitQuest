using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Infrastructure.Repositories
{
    public class UserQuizStatusRepository : RepositoryBase<UserQuizStatus>, IUserQuizStatusRepository
    {
        public UserQuizStatusRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
