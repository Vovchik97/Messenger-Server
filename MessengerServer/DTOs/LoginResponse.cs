namespace MessengerServer.DTOs;

public class LoginResponse
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}