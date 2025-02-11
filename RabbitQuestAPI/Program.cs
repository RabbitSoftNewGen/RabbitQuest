using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitQuestAPI.Application.Interfaces;
using RabbitQuestAPI.Application.Services;
using RabbitQuestAPI.Domain.Entities;
using RabbitQuestAPI.Infrastructure;
using RabbitQuestAPI.Infrastructure.Helpers;
using RabbitQuestAPI.Infrastructure.Repositories;
using RabbitQuestAPI.Infrastructure.Services;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure logging first
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Create a logger factory and logger instance
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

builder.WebHost.UseUrls("http://+:8080");

// Store JWT configuration values
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

logger.LogInformation("Starting authentication configuration...");

builder.Services.AddAuthentication(options =>
{
    logger.LogInformation("Configuring authentication schemes...");
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            logger.LogInformation("JWT: OnMessageReceived event triggered");
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                logger.LogInformation("Authorization header present: {Header}",
                    context.Request.Headers["Authorization"].ToString()[..Math.Min(50, context.Request.Headers["Authorization"].ToString().Length)] + "...");
            }
            else
            {
                logger.LogInformation("No Authorization header present");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            logger.LogInformation("JWT: Token validated successfully");
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
            if (claims != null)
            {
                logger.LogInformation("User Claims: {Claims}", string.Join(", ", claims));
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            logger.LogError(context.Exception, "JWT: Authentication failed");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            logger.LogWarning("JWT: Authentication challenge issued. Error: {Error}, Description: {Description}",
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserQuizStatusRepository, UserQuizStatusRepository>();

logger.LogInformation("Configuring database connection...");

// Configure DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.MigrationsAssembly("RabbitQuestAPI");
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null
            );
        }
    ));

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configure the application
if (app.Environment.IsDevelopment())
{
    logger.LogInformation("Configuring Swagger for development environment");
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowSpecificOrigins");

// Initialize roles and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        logger.LogInformation("Initializing roles and seeding data...");
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await RoleInitializer.InitializeAsync(userManager, roleManager);
        logger.LogInformation("Roles and data seeding completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
        throw; // Rethrow to prevent application startup if seeding fails
    }
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Storage")),
    RequestPath = "/uploads/avatars"
});
// Configure middleware pipeline
logger.LogInformation("Configuring middleware pipeline...");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

logger.LogInformation("Application startup complete. Running the application...");
app.Run();