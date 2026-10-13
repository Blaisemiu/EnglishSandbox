using EnglishSandbox.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishSandbox.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("exercises-with-authors")]
    public async Task<IActionResult> GetExercisesWithAuthors(
        [FromQuery] bool? isPublished = null)
    {
        var query =
            from exercise in _context.Exercises.AsNoTracking()
            join user in _context.Users.AsNoTracking()
                on exercise.AuthorId equals user.Id
            select new
            {
                ExerciseId = exercise.Id,
                exercise.Title,
                exercise.Instruction,
                exercise.IsPublished,
                AuthorId = user.Id,
                AuthorName = user.Name
            };

        if (isPublished.HasValue)
        {
            query = query.Where(item => item.IsPublished == isPublished.Value);
        }

        return Ok(await query.OrderBy(item => item.ExerciseId).ToListAsync());
    }

    [HttpGet("elements-with-details")]
    public async Task<IActionResult> GetElementsWithDetails(
        [FromQuery] int? exerciseId = null)
    {
        var query =
            from element in _context.ExerciseElements.AsNoTracking()
            join exercise in _context.Exercises.AsNoTracking()
                on element.ExerciseId equals exercise.Id
            join user in _context.Users.AsNoTracking()
                on exercise.AuthorId equals user.Id
            select new
            {
                ElementId = element.Id,
                element.Text,
                element.CorrectPosition,
                ExerciseId = exercise.Id,
                ExerciseTitle = exercise.Title,
                AuthorId = user.Id,
                AuthorName = user.Name
            };

        if (exerciseId.HasValue)
        {
            query = query.Where(item => item.ExerciseId == exerciseId.Value);
        }

        var result = await query
            .OrderBy(item => item.ExerciseId)
            .ThenBy(item => item.CorrectPosition)
            .ThenBy(item => item.ElementId)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("user-statistics")]
    public async Task<IActionResult> GetUserStatistics()
    {
        var query =
            from user in _context.Users.AsNoTracking()
            join exercise in _context.Exercises.AsNoTracking()
                on user.Id equals exercise.AuthorId into userExercises
            orderby user.Id
            select new
            {
                UserId = user.Id,
                user.Name,
                user.IsActive,
                TotalExercises = userExercises.Count(),
                PublishedExercises = userExercises.Count(exercise => exercise.IsPublished),
                DraftExercises = userExercises.Count(exercise => !exercise.IsPublished)
            };

        return Ok(await query.ToListAsync());
    }
}
