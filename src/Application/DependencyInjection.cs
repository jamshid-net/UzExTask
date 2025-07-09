using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectTemplate.Application.Common.Behaviours;
using ProjectTemplate.Application.Common.Constants;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Services;
using Serilog;
using Serilog.Events;
using TelegramSink;

namespace ProjectTemplate.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<ICustomIdentityService, CustomIdentityService>();

        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });
    
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var logPath = environment == Environments.Development ? "logs" : $"/var/www/BackendAppLogs/";
        
        var telegramBotToken = builder.Configuration["TelegramBotToken"];
        var telegramChatId = "33780774";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File(@$"{logPath}/log.log", LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();


        TelegramLog.Logger = new LoggerConfiguration().WriteTo
                                                      .TeleSink(telegramBotToken, telegramChatId, null, LogEventLevel.Error)
                                                      .CreateLogger();

    }
}
