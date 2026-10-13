using EnglishSandbox.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using EnglishSandbox.Api.Data;
using EnglishSandbox.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishSandbox.Api.Controllers;

[ApiController]
[Route("api/exercises")]
public class ExercisesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExercisesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] int? authorId = null)
    {
        var query = _context.Exercises.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string text = search.Trim();
            query = query.Where(exercise => exercise.Title.Contains(text));
        }

        if (authorId.HasValue)
        {
            query = query.Where(exercise => exercise.AuthorId == authorId.Value);
        }

        var exercises = await query
            .OrderBy(exercise => exercise.Id)
            .Select(exercise => new
            {
                exercise.Id,
                exercise.AuthorId,
                exercise.Title,
                exercise.Instruction,
                exercise.IsPublished,
                ElementsCount = exercise.Elements.Count
            })
            .ToListAsync();

        return Ok(exercises);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var exercise = await _context.Exercises
            .AsNoTracking()
            .Include(exercise => exercise.Elements)
            .FirstOrDefaultAsync(exercise => exercise.Id == id);

        if (exercise == null)
        {
            return NotFound(new { message = "Задание не найдено." });
        }

        return Ok(exercise);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExerciseRequest request)
    {
        bool authorExists = await _context.Users
            .AnyAsync(user => user.Id == request.AuthorId);

        if (!authorExists)
        {
            return BadRequest(new
            {
                message = "Пользователь с таким AuthorId не найден."
            });
        }

        var exercise = new Exercise { AuthorId = request.AuthorId };

        try
        {
            exercise.Update(request.Title, request.Instruction);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = exercise.Id }, exercise);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExerciseRequest request)
    {
        var exercise = await _context.Exercises
            .Include(exercise => exercise.Elements)
            .FirstOrDefaultAsync(exercise => exercise.Id == id);

        if (exercise == null)
        {
            return NotFound(new { message = "Задание не найдено." });
        }

        bool authorExists = await _context.Users
            .AnyAsync(user => user.Id == request.AuthorId);

        if (!authorExists)
        {
            return BadRequest(new
            {
                message = "Пользователь с таким AuthorId не найден."
            });
        }

        try
        {
            exercise.Update(request.Title, request.Instruction);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        exercise.AuthorId = request.AuthorId;
        await _context.SaveChangesAsync();

        return Ok(exercise);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);

        if (exercise == null)
        {
            return NotFound(new { message = "Задание не найдено." });
        }

        _context.Exercises.Remove(exercise);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var exercise = await _context.Exercises
            .Include(exercise => exercise.Elements)
            .FirstOrDefaultAsync(exercise => exercise.Id == id);

        if (exercise == null)
        {
            return NotFound(new { message = "Задание не найдено." });
        }

        try
        {
            exercise.Publish();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        await _context.SaveChangesAsync();

        return Ok(exercise);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPost("{id:int}/unpublish")]
    public async Task<IActionResult> Unpublish(int id)
    {
        var exercise = await _context.Exercises.FindAsync(id);

        if (exercise == null)
        {
            return NotFound(new { message = "Задание не найдено." });
        }

        exercise.Unpublish();
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class ExerciseRequest
{
    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }

    [Required]
    public string Title { get; set; } = "";

    [Required]
    public string Instruction { get; set; } = "";
}
