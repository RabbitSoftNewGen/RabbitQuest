using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitQuestAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitQuestAPI.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<UserQuizStatus> UserQuizStatuses { get; set; }
        public DbSet<QuizRating> QuizRatings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<QuizRating>()
    .HasKey(qr => qr.Id);

            modelBuilder.Entity<QuizRating>()
                .HasOne(qr => qr.Quiz)
                .WithMany(q => q.Ratings)
                .HasForeignKey(qr => qr.QuizId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QuizRating>()
                .HasOne(qr => qr.User)
                .WithMany(u => u.QuizRatings)
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Category)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CategoryId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(quiz => quiz.Questions)
                .HasForeignKey(q => q.QuizId);

            modelBuilder.Entity<UserQuizStatus>()
                .HasKey(uqs => uqs.Id);

            modelBuilder.Entity<UserQuizStatus>()
                .HasOne(uqs => uqs.User)
                .WithMany(u => u.UserQuizStatuses)
                .HasForeignKey(uqs => uqs.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuizStatus>()
                .HasOne(uqs => uqs.Quiz)
                .WithMany(q => q.UserQuizStatuses)
                .HasForeignKey(uqs => uqs.QuizId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
