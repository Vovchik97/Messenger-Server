namespace MessengerServer.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}