using AutoMapper;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Domain.Entities;

namespace SupportTickets.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        CreateMap<TicketComment, CommentDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.UserRole, o => o.MapFrom(s => s.User.Role.ToString()));

        CreateMap<TicketActivity, ActivityDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName));

        CreateMap<TimeEntry, TimeEntryDto>()
            .ForMember(d => d.AgentName, o => o.MapFrom(s => s.Agent.FullName));

        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.FullName))
            .ForMember(d => d.AssignedAgentName, o => o.MapFrom(s => s.AssignedAgent == null ? null : s.AssignedAgent.FullName));

        CreateMap<Ticket, TicketDetailsDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer.FullName))
            .ForMember(d => d.CustomerEmail, o => o.MapFrom(s => s.Customer.Email))
            .ForMember(d => d.AssignedAgentName, o => o.MapFrom(s => s.AssignedAgent == null ? null : s.AssignedAgent.FullName))
            .ForMember(d => d.TotalTimeMinutes, o => o.MapFrom(s => s.TimeEntries.Sum(t => t.DurationMinutes)))
            .ForMember(d => d.Comments, o => o.MapFrom(s => s.Comments.OrderBy(c => c.CreatedAt)))
            .ForMember(d => d.Timeline, o => o.MapFrom(s => s.Activities.OrderByDescending(a => a.CreatedAt)))
            .ForMember(d => d.TimeEntries, o => o.MapFrom(s => s.TimeEntries.OrderByDescending(t => t.WorkDate)));
    }
}
