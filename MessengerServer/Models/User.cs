using System.ComponentModel.DataAnnotations;

namespace MessengerServer.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50), MinLength(3)]
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
}