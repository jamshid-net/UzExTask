namespace ProjectTemplate.Shared.Extensions;
public static class DateFormat
{
    /// <summary>
    ///"MM.dd.yyyy HH:mm",
    ///"MM.dd.yyyy HH:mm:ss",
    ///"MM.dd.yyyy HH",
    ///"MM.dd.yyyy",
    ///"yyyy-MM-ddTHH:mm:ss.fffZ",
    ///"yyyy-MM-ddTHH:mm:ssZ",
    ///"yyyy-MM-ddTHH:mm:ss"
    /// </summary>
    public static string[] ReadDateFormats { get ; } = [
        "MM.dd.yyyy HH:mm",
        "MM.dd.yyyy HH:mm:ss",
        "MM.dd.yyyy HH",
        "MM.dd.yyyy",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss"
    ];
    /// <summary>
    /// MM.dd.yyyy HH:mm zzz
    /// </summary>
    public static string WriteDateFormat { get ; } = "MM.dd.yyyy HH:mm zzz";
}
