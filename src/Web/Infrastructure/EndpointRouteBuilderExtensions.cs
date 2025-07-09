using System.Diagnostics.CodeAnalysis;


namespace ProjectTemplate.Web.Infrastructure;

public static class EndpointRouteBuilderExtensions
{
    public static RouteHandlerBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);
        var route = string.IsNullOrWhiteSpace(pattern)
            ? "/" + handler.Method.Name
            : pattern.StartsWith("/") ? pattern : "/" + pattern;

        return builder.MapGet(route, handler)
                      .WithName(handler.Method.Name)
                      .WithOpenApi();
    }

    public static RouteHandlerBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        var route = string.IsNullOrWhiteSpace(pattern)
            ? "/" + handler.Method.Name
            : pattern.StartsWith("/") ? pattern : "/" + pattern;

        return builder.MapPost(route, handler)
                      .WithName(handler.Method.Name)
                      .WithOpenApi();
    }

    public static RouteHandlerBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        var route = string.IsNullOrWhiteSpace(pattern)
            ? "/" + handler.Method.Name
            : pattern.StartsWith("/") ? pattern : "/" + pattern;

        return builder.MapPut(route, handler)
                      .WithName(handler.Method.Name)
                      .WithOpenApi();
    }

    public static RouteHandlerBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        var route = string.IsNullOrWhiteSpace(pattern)
            ? "/" + handler.Method.Name
            : pattern.StartsWith("/") ? pattern : "/" + pattern;

        return builder.MapDelete(route, handler)
                      .WithName(handler.Method.Name)
                      .WithOpenApi();
    }
}

