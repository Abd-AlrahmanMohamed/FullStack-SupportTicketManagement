using MediatR;
using SupportTickets.Application.Common.Dtos;

namespace SupportTickets.Application.Features.Dashboard.Queries.GetDashboard;

public class GetDashboardQuery : IRequest<DashboardDto>
{
}
