using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Application.Services;
using RabbitQuestAPI.Domain.Entities;
using System.Security.Claims;

namespace RabbitQuestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly IQuizRepository _quizRepository;
        private readonly ICategoryRepository _categoryRepository; // Add category repository
        private readonly ILogger<QuizController> _logger;


        public QuizController(IQuizService quizService, IQuizRepository quizRepository, ICategoryRepository categoryRepository, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _quizRepository = quizRepository;
            _categoryRepository = categoryRepository; // Inject category repository
            _logger = logger;

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            try
            {
                // Find the quiz and include related questions and category
                var quiz = await _quizRepository.GetQueryable()
                    .Include(q => q.Questions) // Include related questions
                    .Include(q => q.Category)  // Include the category of the quiz
                    .FirstOrDefaultAsync(q => q.Id == id);

                if (quiz == null)
                {
                    return NotFound("Quiz not found");
                }

                // Map the quiz to QuizDto
                var quizDto = new GetQuizDto
                {
                    Title = quiz.Title,
                    Description = quiz.Description,
                    Category = new CategoryDto
                    {
                        Id = quiz.Category.Id,
                        Name = quiz.Category.Name
                    },
                    Questions = quiz.Questions.Select(q => new QuestionDto
                    {
                        Title = q.Title,
                        Points = q.Points,
                        TimeLimit = q.TimeLimit,
                        Image = q.Image,
                        Video = q.Video,
                        Answers = q.Answers?.ToList() // Ensure variants are included as a list
                    }).ToList() // Map questions to QuestionDto
                };

                return Ok(quizDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz by id.");
                return StatusCode(500, "An error occurred while fetching the quiz.");
            }
        }





        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto createQuizDto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("No NameIdentifier claim found in token");
                    return Unauthorized("User ID not found in token");
                }
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Invalid user ID format");
                }

                var category = await _categoryRepository.GetQueryable()
                    .FirstOrDefaultAsync(c => c.Name == createQuizDto.Category);
                if (category == null)
                {
                    category = new Category { Name = createQuizDto.Category };
                    await _categoryRepository.AddAsync(category);
                    await _categoryRepository.SaveChangesAsync();
                }

                var quiz = new Quiz
                {
                    Title = createQuizDto.Title,
                    Description = createQuizDto.Description,
                    CategoryId = category.Id,
                    UserId = userId,
                    Rating = 0,
                    Questions = createQuizDto.Questions.Select(questionDto => new Question
                    {
                        Title = questionDto.Title,
                        Points = questionDto.Points,
                        TimeLimit = questionDto.TimeLimit,
                        CorrectAnswers = questionDto.CorrectAnswers.ToList(),
                        Image = questionDto.Image,
                        Video = questionDto.Video,
                        Answers = questionDto.Answers?.ToList()
                    }).ToList(),
                    UserQuizStatuses = new List<UserQuizStatus>()
                };

             
                quiz.UserQuizStatuses.Add(new UserQuizStatus
                {
                    UserId = userId,
                    QuizStatus = QuizStatus.Created
                });

                await _quizRepository.AddAsync(quiz);
                await _quizRepository.SaveChangesAsync();

                return Ok(new { QuizId = quiz.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quiz.");
                return StatusCode(500, "An error occurred while creating the quiz.");
            }
        }
    }
}