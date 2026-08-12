using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SupportTickets.Application.Common.Behaviours;

namespace SupportTickets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
       services.AddAutoMapper(cfg => { },typeof(DependencyInjection).Assembly);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        return services;
    }
}
