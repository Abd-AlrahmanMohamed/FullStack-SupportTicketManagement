using AutoMapper;
using SupportTickets.Application.Common.Mappings;

namespace SupportTickets.Tests.TestUtilities;

public static class MapperFactory {
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        return configuration.CreateMapper();
    }
}
