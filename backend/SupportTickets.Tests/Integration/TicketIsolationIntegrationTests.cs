using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Models;
using SupportTickets.Application.Features.Auth.Commands.Login;
using Xunit;

namespace SupportTickets.Tests.Integration;

public class TicketIsolationIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public TicketIsolationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand { Email = email, Password = password });
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with {response.StatusCode}: {content}");
        }

        var body = await response.Content.ReadFromJsonAsync<LoginResultDto>(JsonOptions);
        return body!.Token;
    }

    private static void Authorize(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUser()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand
        {
            Email = "admin@support.local",
            Password = "Admin@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResultDto>(JsonOptions);
        body!.Token.Should().NotBeNullOrEmpty();
        body.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginCommand
        {
            Email = "admin@support.local",
            Password = "wrong-password"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTickets_WithoutToken_Returns401()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTicket_AsCustomer_IgnoresSpoofedCustomerIdAndUsesTokenIdentity()
    {
        var client = CreateClient();
        var token = await LoginAsync(client, "customer@support.local", "Customer@123");
        Authorize(client, token);

        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Integration test ticket",
            description = "Created via integration test",
            priority = "Low",
            customerId = Guid.Empty // attempted spoof - must be ignored by the server
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ticket = await response.Content.ReadFromJsonAsync<TicketDto>(JsonOptions);
        ticket!.CustomerId.Should().NotBe(Guid.Empty);
        ticket.CustomerName.Should().Be("Dave Customer");
    }

    [Fact]
    public async Task GetTickets_AsCustomer_OnlyReturnsOwnTickets()
    {
        var client = CreateClient();
        var token = await LoginAsync(client, "customer@support.local", "Customer@123");
        Authorize(client, token);

        var response = await client.GetAsync("/api/tickets?pageSize=50");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PaginatedResult<TicketDto>>(JsonOptions);
        page!.Items.Should().OnlyContain(t => t.CustomerName == "Dave Customer");
    }

    [Fact]
    public async Task CustomerB_CannotGetCustomerAsTicketById_Returns404()
    {
        var client = CreateClient();

        // Customer A (Dave) finds one of their own ticket IDs.
        var customerAToken = await LoginAsync(client, "customer@support.local", "Customer@123");
        Authorize(client, customerAToken);
        var listResponse = await client.GetAsync("/api/tickets?pageSize=50");
        var page = await listResponse.Content.ReadFromJsonAsync<PaginatedResult<TicketDto>>(JsonOptions);
        var customerATicketId = page!.Items.First().Id;

        // Customer B (Erin) tries to fetch it directly by ID.
        var customerBClient = CreateClient();
        var customerBToken = await LoginAsync(customerBClient, "customer2@support.local", "Customer@123");
        Authorize(customerBClient, customerBToken);

        var response = await customerBClient.GetAsync($"/api/tickets/{customerATicketId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CustomerB_CannotCommentOnCustomerAsTicket_Returns404()
    {
        var client = CreateClient();

        var customerAToken = await LoginAsync(client, "customer@support.local", "Customer@123");
        Authorize(client, customerAToken);
        var listResponse = await client.GetAsync("/api/tickets?pageSize=50");
        var page = await listResponse.Content.ReadFromJsonAsync<PaginatedResult<TicketDto>>(JsonOptions);
        var customerATicketId = page!.Items.First().Id;

        var customerBClient = CreateClient();
        var customerBToken = await LoginAsync(customerBClient, "customer2@support.local", "Customer@123");
        Authorize(customerBClient, customerBToken);

        var response = await customerBClient.PostAsJsonAsync($"/api/tickets/{customerATicketId}/comments", new
        {
            message = "Trying to sneak a comment onto someone else's ticket."
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_CannotAccess_AdminOnlyUsersEndpoint()
    {
        var client = CreateClient();
        var token = await LoginAsync(client, "customer@support.local", "Customer@123");
        Authorize(client, token);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
