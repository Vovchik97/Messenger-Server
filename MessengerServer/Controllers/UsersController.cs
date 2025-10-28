using MessengerServer.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly MessengerDbContext _context;
    
    public UsersController(MessengerDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var currentUserIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var users = await _context.Users
            .Where(u => u.Id != currentUserId)
            .Select(u => new
            {
                u.Id,
                u.Username
            })
            .ToListAsync();

        return Ok(users);
    }
}