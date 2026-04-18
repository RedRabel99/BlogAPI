using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace BlogAPI.IntegrationTests.Infrastructure;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbcContainer;

    public IntegrationTestFactory()
    {
        _dbcContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("blogapi-db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    }

    public async Task InitializeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
