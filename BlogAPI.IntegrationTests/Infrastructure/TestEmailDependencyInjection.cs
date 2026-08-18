using BlogAPI.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BlogAPI.IntegrationTests.Infrastructure;

public static class TestEmailDependencyInjection
{
    public static IServiceCollection AddTestEmailSender(this IServiceCollection services)
    {
        // deregistering the OutboxEmailProcessor background service to prevent it from running during integration tests
        var emailProcessor = services.SingleOrDefault(s =>
            s.ServiceType == typeof(IHostedService) &&
            s.ImplementationType == typeof(OutboxEmailProcessor));

        if (emailProcessor is not null)
        {
            services.Remove(emailProcessor);
        }

        services.RemoveAll<IEmailSender>();
        services.AddSingleton<TestEmailSender>();
        services.AddSingleton<IEmailSender>(sp => sp.GetRequiredService<TestEmailSender>());

        return services;
    }
}
