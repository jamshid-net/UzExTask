using Microsoft.AspNetCore.Mvc;
using NSwag;
using NSwag.Generation.Processors.Security;
using ProjectTemplate.Application.Common.Exceptions;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Infrastructure.Data;
using ProjectTemplate.Web.Services;
using ProjectTemplate.Web.WebSocket.Services;

namespace ProjectTemplate.Web;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<ICurrentUser, CurrentCurrentUser>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);
        builder.Services.AddProblemDetails(); 
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApiDocument((configure, sp) =>
        {
            configure.Title = "ProjectTemplate API";

            // Add JWT
            configure.AddSecurity("JWT", [], new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.ApiKey,
                Name = "Authorization",
                In = OpenApiSecurityApiKeyLocation.Header,
                Description = "Type into the textbox: Bearer {your JWT token}."
            });

            configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
        builder.Services.AddSingleton<IHubConnectionManager, HubConnectionManager>();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<INotifyHubService, NotifyHubService>();
    }

}
