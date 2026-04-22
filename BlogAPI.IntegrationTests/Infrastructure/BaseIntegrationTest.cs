using Microsoft.Extensions.DependencyInjection;
using BlogAPI.Infrastructure;
using BlogAPI.IntegrationTests.TestData;
using BlogAPI.Application.DTOs.Auth;
using System.Net.Http.Json;
using System.Globalization;

namespace BlogAPI.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly HttpClient HttpClient;
    protected readonly AppDbContext AppDbContext;
    protected readonly IUserManager UserManager;
    protected readonly TestDataSeeder DataSeeder;

    protected BaseIntegrationTest(IntegrationTestFactory factory)
    {
        _scope = factory.Services.CreateScope();
        HttpClient = factory.CreateClient();
        AppDbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        UserManager = _scope.ServiceProvider.GetRequiredService<IUserManager>();
        DataSeeder = new TestDataSeeder(AppDbContext, UserManager);
    }

    public async Task AuthenticateAsync(string email, string password)
    {
        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };
        var response = await HttpClient.PostAsJsonAsync("/auth/login", loginDto);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<string>();
        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse);
    }
    public Task InitializeAsync()
    {
        return DataSeeder.SeedAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
