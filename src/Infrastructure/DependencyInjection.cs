using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Services;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Infrastructure.Data.Interceptors;
using ProjectTemplate.Infrastructure.Services;
using StackExchange.Redis;

namespace ProjectTemplate.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("UzExTaskDb");
        Guard.Against.Null(connectionString, message: "Connection string 'ProjectTemplateDb' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention()
                .AddAsyncSeeding(sp);
        });
        
        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitializer>();
        
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "localhost"; 
            options.InstanceName = "UzExTask:";
        });

        builder.Services.AddScoped<IAuthCacheService, AuthCacheService>();

        builder.Services.AddSingleton<IRedisServer>(_ =>
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect("localhost", cfg =>
            {
                var cfa = cfg;
            });
            var endPoint = connectionMultiplexer.GetEndPoints().FirstOrDefault();
            var server = connectionMultiplexer.GetServer(endPoint);
            return new RedisServer(server);
        });


    }
}
