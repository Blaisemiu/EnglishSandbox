using EnglishSandbox.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using EnglishSandbox.Api.Data;
using EnglishSandbox.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishSandbox.Api.Controllers;

[ApiController]
[Route("api/exercise-elements")]
public class ExerciseElementsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExerciseElementsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? exerciseId = null)
    {
        var query = _context.ExerciseElements.AsNoTracking();

        if (exerciseId.HasValue)
        {
            query = query.Where(element => element.ExerciseId == exerciseId.Value);
        }

        var elements = await query
            .OrderBy(element => element.ExerciseId)
            .ThenBy(element => element.CorrectPosition)
            .ThenBy(element => element.Id)
            .ToListAsync();

        return Ok(elements);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var element = await _context.ExerciseElements
            .AsNoTracking()
            .FirstOrDefaultAsync(element => element.Id == id);

        if (element == null)
        {
            return NotFound(new { message = "Элемент не найден." });
        }

        return Ok(element);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ExerciseElementRequest request)
    {
        var exercise = await _context.Exercises.FindAsync(request.ExerciseId);

        if (exercise == null)
        {
            return BadRequest(new { message = "Задание с таким ExerciseId не найдено." });
        }

        var element = new ExerciseElement { ExerciseId = request.ExerciseId };

        try
        {
            element.Update(request.Text, request.CorrectPosition);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        exercise.Unpublish();
        _context.ExerciseElements.Add(element);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = element.Id }, element);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExerciseElementRequest request)
    {
        var element = await _context.ExerciseElements.FindAsync(id);

        if (element == null)
        {
            return NotFound(new { message = "Элемент не найден." });
        }

        var exercise = await _context.Exercises.FindAsync(request.ExerciseId);

        if (exercise == null)
        {
            return BadRequest(new { message = "Задание с таким ExerciseId не найдено." });
        }

        try
        {
            element.Update(request.Text, request.CorrectPosition);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        if (element.ExerciseId != request.ExerciseId)
        {
            var previousExercise = await _context.Exercises.FindAsync(element.ExerciseId);
            previousExercise?.Unpublish();
        }

        element.ExerciseId = request.ExerciseId;
        exercise.Unpublish();
        await _context.SaveChangesAsync();

        return Ok(element);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var element = await _context.ExerciseElements.FindAsync(id);

        if (element == null)
        {
            return NotFound(new { message = "Элемент не найден." });
        }

        var exercise = await _context.Exercises.FindAsync(element.ExerciseId);
        exercise?.Unpublish();

        _context.ExerciseElements.Remove(element);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class ExerciseElementRequest
{
    [Range(1, int.MaxValue)]
    public int ExerciseId { get; set; }

    [Required]
    public string Text { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int CorrectPosition { get; set; }
}
