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

        public QuizService(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        public async Task<IEnumerable<UserQuizStatus>> GetUserQuizStatusesAsync(int userId)
        {
            return await _quizRepository.GetQueryable()
                .SelectMany(q => q.UserQuizStatuses)
                .Where(uqs => uqs.UserId == userId)
                .ToListAsync();
        }



        public async Task<IEnumerable<QuizDto>> GetAllQuizzesAsync(string? searchTerm = null)
        {
            var quizzes = await _quizRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                quizzes = quizzes.Where(q => EF.Functions.FreeText(q.Title, searchTerm)
                          || EF.Functions.FreeText(q.Description, searchTerm));
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
