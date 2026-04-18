using Microsoft.Extensions.DependencyInjection;
using BlogAPI.Infrastructure;
using BlogAPI.IntegrationTests.TestData;

namespace BlogAPI.IntegrationTests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    private readonly IServiceScope _scope;
    protected readonly HttpClient HttpClient;
    protected readonly AppDbContext AppDbContext;
    protected readonly TestDataSeeder DataSeeder;

    protected BaseIntegrationTest(IntegrationTestFactory factory)
    {
        _scope = factory.Services.CreateScope();
        HttpClient = factory.CreateClient();
        AppDbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = _scope.ServiceProvider.GetRequiredService<IUserManager>();
        DataSeeder = new TestDataSeeder(AppDbContext, userManager);
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
