namespace SupportTickets.Application.Common.Dtos;

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
