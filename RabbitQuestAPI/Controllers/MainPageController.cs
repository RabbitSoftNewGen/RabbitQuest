using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Application.DTO; 
using RabbitQuestAPI.Infrastructure;
using System.Linq; 
using Microsoft.AspNetCore.Authorization;
using RabbitQuestAPI.Domain.Entities;

namespace RabbitQuestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MainPageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MainPageController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<ActionResult<IEnumerable<QuizDto>>> GetAllUsers(string? searchTerm = null)
        {
            var query = _context.Users
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(q => EF.Functions.FreeText(q.UserName, searchTerm));

            }

            var quizzes = await query
                .Select(q => new UserDto
                {
                   Username = q.UserName,
                    AvatarURL = q.AvatarURL
                })
                .ToListAsync();

            return Ok(quizzes);
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