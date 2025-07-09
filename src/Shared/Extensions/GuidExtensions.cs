namespace ProjectTemplate.Shared.Extensions;
public static class GuidExtensions
{
    public static string ToUpperString(this Guid arg) => arg.ToString("N").ToUpper();
    public static string ToLowerString(this Guid arg) => arg.ToString("N").ToLower();

}
