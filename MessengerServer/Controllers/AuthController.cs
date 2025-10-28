using MessengerServer.Data;
using MessengerServer.DTOs;
using MessengerServer.Models;
using MessengerServer.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoginRequest = MessengerServer.DTOs.LoginRequest;
using RegisterRequest = MessengerServer.DTOs.RegisterRequest;

namespace MessengerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MessengerDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(MessengerDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
        {
            return BadRequest("Username already exists");
        }
        
        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(Login), new { }, new { Id = user.Id, Username = user.Username });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = _tokenService.GenerateToken(user.Id, user.Username);
        return Ok(new LoginResponse { UserId = user.Id, Token = token });
    }
}