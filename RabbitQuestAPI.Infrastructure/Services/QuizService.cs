using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Application.Services;
using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RabbitQuestAPI.Infrastructure.Services
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly IUserQuizStatusRepository _userQuizStatusRepository;

        public QuizService(IQuizRepository quizRepository, IUserQuizStatusRepository userQuizStatusRepository)
        {
            _quizRepository = quizRepository;
            _userQuizStatusRepository = userQuizStatusRepository;
        }

        public async Task<IEnumerable<UserQuizStatus>> GetUserQuizStatusesAsync(int userId)
        {
            return await _userQuizStatusRepository.GetQueryable()
                .Include(uqs => uqs.Quiz)
                    .ThenInclude(q => q.Category)
                .Where(uqs => uqs.UserId == userId)
                .ToListAsync();
        }



        public async Task<IEnumerable<QuizDto>> GetAllQuizzesAsync(string? searchTerm = null)
        {
            var quizzes = await _quizRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                quizzes = quizzes.Where(q => EF.Functions.Like(q.Title, $"%{searchTerm}%"))
                                .Union(quizzes.Where(q => EF.Functions.Like(q.Description, $"%{searchTerm}%")));
            }

            return  quizzes.Select(q => new QuizDto
            {
                Id = q.Id,
                Title = q.Title,
                Description = q.Description,
                Category = new CategoryDto
                {
                    Id = q.Category.Id,
                    Name = q.Category.Name
                },

            }).ToList();
        }
    }
}
