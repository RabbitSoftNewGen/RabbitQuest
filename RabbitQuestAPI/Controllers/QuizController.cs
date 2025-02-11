using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Application.DTO;
using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Application.Services;
using RabbitQuestAPI.Domain.Entities;
using System.Security.Claims;
using System.Text.Json;

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
                    .Include(q => q.Category) 
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
                    Rating = quiz.Rating,
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
        public async Task<IActionResult> CreateQuiz([FromBody] JsonElement createQuizJson)
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

                // Отримуємо заголовок, категорію та опис
                string title = createQuizJson.GetProperty("title").GetString();
                string description = createQuizJson.GetProperty("description").GetString();

                // Обробляємо категорію
                string categoryName = createQuizJson.TryGetProperty("category", out var categoryElement)
                    ? categoryElement.GetString()
                    : "Uncategorized";

                var category = await _categoryRepository.GetQueryable()
                    .FirstOrDefaultAsync(c => c.Name == categoryName);
                if (category == null)
                {
                    category = new Category { Name = categoryName };
                    await _categoryRepository.AddAsync(category);
                    await _categoryRepository.SaveChangesAsync();
                }

                // Отримуємо питання
                var questions = new List<Question>();
                if (createQuizJson.TryGetProperty("questions", out JsonElement questionsElement) && questionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var questionElement in questionsElement.EnumerateArray())
                    {
                        string questionTitle = questionElement.GetProperty("title").GetString();
                        int timeLimit = questionElement.GetProperty("time").GetInt32();
                        string image = questionElement.TryGetProperty("image", out var imgElem) && imgElem.ValueKind != JsonValueKind.Null
                            ? imgElem.GetString()
                            : null;

                        List<string> answers = new();
                        List<string> correctAnswers = new();

                        if (questionElement.TryGetProperty("answers", out JsonElement answersElement))
                        {
                            if (answersElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var answer in answersElement.EnumerateArray())
                                {
                                    if (answer.ValueKind == JsonValueKind.String)
                                    {
                                        
                                        answers.Add(answer.GetString());
                                    }
                                    else if (answer.ValueKind == JsonValueKind.Object)
                                    {
                                        // Варіант з множинним вибором
                                        string answerText = answer.GetProperty("title").GetString();
                                        bool isCorrect = answer.GetProperty("isCorrect").GetBoolean();
                                        answers.Add(answerText);
                                        if (isCorrect) correctAnswers.Add(answerText);
                                    }
                                }
                            }
                        }

                        questions.Add(new Question
                        {
                            Title = questionTitle,
                            Points = 0,
                            TimeLimit = timeLimit,
                            Image = image,
                            Answers = answers,
                            CorrectAnswers = correctAnswers
                        });
                    }
                }

                var quiz = new Quiz
                {
                    Title = title,
                    Description = description,
                    CategoryId = category.Id,
                    UserId = userId,
                    Rating = 0,
                    Questions = questions,
                    UserQuizStatuses = new List<UserQuizStatus>
            {
                new UserQuizStatus
                {
                    UserId = userId,
                    QuizStatus = QuizStatus.Created
                }
            }
                };

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



        [HttpPost("rate")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RateQuiz([FromBody] RateQuizDto rateQuizDto)
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

                var quiz = await _quizRepository.GetQueryable()
                    .Include(q => q.UserQuizStatuses)
                    .Include(q => q.Ratings)
                    .FirstOrDefaultAsync(q => q.Id == rateQuizDto.QuizId);

                if (quiz == null)
                {
                    return NotFound("Quiz not found");
                }

                // Check if user has completed the quiz
                var userStatus = quiz.UserQuizStatuses
                    .FirstOrDefault(s => s.UserId == userId);

                //if (userStatus == null || userStatus.QuizStatus != QuizStatus.Completed)
                //{
                //    return BadRequest("You must complete the quiz before rating it");
                //}

               
                //var existingRating = quiz.Ratings?.FirstOrDefault(r => r.UserId == userId);

                //if (existingRating != null)
                //{
                  
                //    existingRating.Rating = rateQuizDto.Rating;
 
                //}
                //else
                //{
                    // Add new rating
                    if (quiz.Ratings == null)
                    {
                        quiz.Ratings = new List<QuizRating>();
                    }

                    quiz.Ratings.Add(new QuizRating
                    {
                        QuizId = quiz.Id,
                        UserId = userId,
                        Rating = rateQuizDto.Rating,
          
                    });
                //}

                // Calculate new average rating
                quiz.Rating = quiz.Ratings.Average(r => r.Rating);

                await _quizRepository.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Quiz rated successfully",
                    NewRating = quiz.Rating,
                    TotalRatings = quiz.Ratings.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rating quiz");
                return StatusCode(500, "An error occurred while rating the quiz");
            }
        }
    }
}