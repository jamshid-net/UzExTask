using System.Reflection;
using ProjectTemplate.Shared.Extensions;

namespace ProjectTemplate.Web.Infrastructure;

public static class WebApplicationExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group)
    {
        var groupName = group.GetType().Name;
        var assembly = Assembly.GetExecutingAssembly();
        var t = assembly.GetName();

        return app
            .MapGroup($"/api/web/{groupName}") 
            .WithGroupName(groupName)
            .WithTags(groupName.FromCamelCaseToSnakeCase());
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                var groupBuilder = app.MapGroup(instance);
                instance.Map(groupBuilder);
            }
        }

        return app;
    }
}
