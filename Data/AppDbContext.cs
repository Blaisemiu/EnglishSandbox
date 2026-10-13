using EnglishSandbox.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnglishSandbox.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Exercise> Exercises => Set<Exercise>();

    public DbSet<ExerciseElement> ExerciseElements =>
        Set<ExerciseElement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exercise>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(exercise => exercise.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Exercise>()
            .HasMany(exercise => exercise.Elements)
            .WithOne()
            .HasForeignKey(element => element.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}