using Serilog;

namespace ProjectTemplate.Application.Common.Constants;
public static class TelegramLog
{
    static ILogger s_logger = Serilog.Core.Logger.None;
    public static ILogger Logger
    {
        get => s_logger;
        set => s_logger = value is null ? throw new ArgumentNullException("Telegram logger configure not set") : value;
    }

    public static void LogError(string message)
    {
        if (s_logger == Serilog.Core.Logger.None)
        {
            Console.WriteLine("⚠️ Telegram logger not configured.");
            return;
        }
        s_logger.Error(message);
    }
   
}
