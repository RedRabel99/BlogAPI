using BlogAPI.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace BlogAPI.IntegrationTests.Infrastructure;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbcContainer;

    public IntegrationTestFactory()
    {
        _dbcContainer = new PostgreSqlBuilder("postgres:latest")
            .WithDatabase("blogapi-db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => 
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if(descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbcContainer.GetConnectionString())
            );
        });
    }
    public async Task InitializeAsync()
    {
        await _dbcContainer.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new Task DisposeAsync()
    {
        return _dbcContainer.StopAsync();
    }
}
