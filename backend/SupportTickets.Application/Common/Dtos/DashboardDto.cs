namespace SupportTickets.Application.Common.Dtos;

public class DashboardDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int OpenCriticalTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public List<AgentWorkloadDto> AgentWorkload { get; set; } = new();
}

public class AgentWorkloadDto
{
    public string AgentName { get; set; } = string.Empty;
    public int ActiveTickets { get; set; }
}
