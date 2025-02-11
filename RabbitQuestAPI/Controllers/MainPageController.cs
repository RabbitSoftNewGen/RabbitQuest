using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Application.DTO; 
using RabbitQuestAPI.Infrastructure;
using System.Linq; 
using Microsoft.AspNetCore.Authorization;
using RabbitQuestAPI.Domain.Entities;
using RabbitQuestAPI.Infrastructure.Repositories;
using RabbitQuestAPI.Application.Interfaces;

namespace RabbitQuestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainPageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserRepository _userRepository;

        public MainPageController(ApplicationDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        [HttpGet("quizzes")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetAllQuizzes(string? searchTerm = null)
        {
            var query = _context.Quizzes
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(q => EF.Functions.FreeText(q.Title, searchTerm)
                                      || EF.Functions.FreeText(q.Description, searchTerm));
            }

            var quizzes = await query
                .Select(q => new QuizDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Description = q.Description,
                    Rating = q.Rating,
                    Category = new CategoryDto
                    {
                        Id = q.Category.Id,
                        Name = q.Category.Name
                    }
                })
                .ToListAsync();

            return Ok(quizzes);
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(string? searchTerm = null)
        {
            var query = _userRepository.GetQueryable()
                .Include(u => u.UserQuizStatuses)
                    .ThenInclude(uqs => uqs.Quiz)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(q => EF.Functions.Like(q.UserName, $"%{searchTerm}%"));
            }

            var users = await query
                .Select(u => new UserDto
                {
                    Username = u.UserName,
                    AvatarURL = u.AvatarURL,
                    Rating = u.UserQuizStatuses
                        .Where(uqs => uqs.QuizStatus == QuizStatus.Created && uqs.Quiz != null)
                        .Select(uqs => uqs.Quiz.Rating)
                        .DefaultIfEmpty()
                        .Average()
                })
                .ToListAsync();

            return Ok(users);
        }


        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
                .ToListAsync();
            return Ok(categories);

        }


    }

   
}