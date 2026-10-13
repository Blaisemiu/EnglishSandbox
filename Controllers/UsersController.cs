using EnglishSandbox.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using EnglishSandbox.Api.Data;
using EnglishSandbox.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishSandbox.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string text = search.Trim();

            query = query.Where(user => user.Name.Contains(text));
        }

        var users = await query
            .OrderBy(user => user.Id)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                user.IsActive
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        return Ok(user);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] UserRequest request)
    {
        var user = new User();

        try
        {
            user.UpdateProfile(request.Name, request.Email);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }

        if (!request.IsActive)
        {
            user.Deactivate();
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UserRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        try
        {
            user.UpdateProfile(request.Name, request.Email);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }

        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [Authorize(Roles = AuthSettings.AdminRole)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Пользователь не найден."
            });
        }

        bool hasExercises = await _context.Exercises
            .AnyAsync(exercise => exercise.AuthorId == id);

        if (hasExercises)
        {
            return Conflict(new
            {
                message = "Нельзя удалить пользователя, у которого есть задания."
            });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UserRequest
{
    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Email { get; set; } = "";

    public bool IsActive { get; set; } = true;
}