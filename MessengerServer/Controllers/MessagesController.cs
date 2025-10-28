using MessengerServer.Data;
using MessengerServer.DTOs;
using MessengerServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MessengerDbContext _context;

    public MessagesController(MessengerDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }
        
        var message = new Message
        {
            SenderId = userId,
            ReceiverId = request.ToUserId,
            Content = request.Content
        };


        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new MessageDto
        {
            Id = message.Id,
            From = message.SenderId,
            To = message.ReceiverId,
            Content = message.Content,
            Date = message.Date
        });
    }

    [HttpGet("{otherUserId}")]
    public async Task<IActionResult> GetMessages(int otherUserId)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var messages = await _context.Messages
            .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == userId))
            .OrderBy(m => m.Date)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                From = m.SenderId,
                To = m.ReceiverId,
                Content = m.Content,
                Date = m.Date
            })
            .ToListAsync();
        
        return Ok(messages);
    }

    [HttpGet("chats")]
    public async Task<IActionResult> GetChats()
    {
        var currentUserIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(currentUserIdClaim) || !int.TryParse(currentUserIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var chats = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => new
            {
                OtherUserId = g.Key,
                LastMessage = g.OrderByDescending(m => m.Date).FirstOrDefault()
            })
            .ToListAsync();

        var otherUserIds = chats.Select(c => c.OtherUserId).ToList();
        var otherUsers = await _context.Users
            .Where(u => otherUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Username);

        var result = chats.Select(c => new
            {
                OtherUserId = c.OtherUserId,
                Username = otherUsers.GetValueOrDefault(c.OtherUserId, "Unknown"),
                LastMessage = c.LastMessage.Content,
                Date = c.LastMessage.Date
            })
            .OrderByDescending(x => x.Date)
            .ToList();

        return Ok(result);
    }
}

public class SendMessageRequest
{
    public int ToUserId { get; set; }
    public string Content { get; set; } = string.Empty;
}