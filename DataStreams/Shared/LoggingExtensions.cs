using System.Diagnostics;

namespace DataStreams.Shared;

#region Logger Extensions Using Color When Logger is a Console
/// <summary>
/// This class uses on the ConsoleExtensions class below to use color
/// when logging to the console. (The default logger only uses color
/// for the beginning of the logged text, this extension logs the full text in color.)
/// </summary>
public static class LoggingExtensions
{
    public static bool UseColoring = false;

    #region LogInformation
    public static void LogInformationExt<T>(this ILogger<T> logger, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Blue, LogLevel.Information, null, null, args);
    }

    public static void LogInformationExt<T>(this ILogger<T> logger, EventId eventId, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Blue, LogLevel.Information, eventId, null, args);
    }

    public static void LogInformationExt<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Blue, LogLevel.Information, null, exception, args);
    }

    public static void LogInformationExt<T>(this ILogger<T> logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Blue, LogLevel.Information, eventId, exception, args);
    }
    #endregion

    #region LogWarning

    public static void LogWarningExt<T>(this ILogger<T> logger, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Yellow, LogLevel.Warning, null, null, args);
    }

    public static void LogWarningExt<T>(this ILogger<T> logger, EventId eventId, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Yellow, LogLevel.Warning, eventId, null, args);
    }

    public static void LogWarningExt<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Yellow, LogLevel.Warning, null, exception, args);
    }

    public static void LogWarningExt<T>(this ILogger<T> logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Yellow, LogLevel.Warning, eventId, exception, args);
    }
    #endregion

    #region LogError

    public static void LogErrorExt<T>(this ILogger<T> logger, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Error, null, null, args);
    }

    public static void LogErrorExt<T>(this ILogger<T> logger, EventId eventId, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Error, eventId, null, args);
    }

    public static void LogErrorExt<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Error, null, exception, args);
    }

    public static void LogErrorExt<T>(this ILogger<T> logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Error, eventId, exception, args);
    }
    #endregion

    #region LogCritical
    public static void LogCriticalExt<T>(this ILogger<T> logger, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Critical, null, null, args);
    }

    public static void LogCriticalExt<T>(this ILogger<T> logger, EventId eventId, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Critical, eventId, null, args);
    }

    public static void LogCriticalExt<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Critical, null, exception, args);
    }

    public static void LogCriticalExt<T>(this ILogger<T> logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Red, LogLevel.Critical, eventId, exception, args);
    }
    #endregion

    #region LogDebug
    public static void LogDebugExt<T>(this ILogger<T> logger, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Green, LogLevel.Debug, null, null, args);
    }

    public static void LogDebugExt<T>(this ILogger<T> logger, EventId eventId, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Green, LogLevel.Debug, eventId, null, args);
    }

    public static void LogDebugExt<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Green, LogLevel.Debug, null, exception, args);
    }

    public static void LogDebugExt<T>(this ILogger<T> logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Green, LogLevel.Debug, eventId, exception, args);
    }
    #endregion

    #region LogTrace
    public static void LogTraceExt<T>(this ILogger<T> logger, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.Cyan, LogLevel.Trace, null, null, args);
    }

    public static void LogTraceExt<T>(this ILogger<T> logger, EventId eventId, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.White, LogLevel.Trace, eventId, null, args);
    }

    public static void LogTraceExt<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.White, LogLevel.Trace, null, exception, args);
    }

    public static void LogTraceExt<T>(this ILogger<T> logger, EventId eventId, Exception exception, string message, params object[] args)
    {
        LogColored(logger, message, ConsoleColor.White, LogLevel.Trace, eventId, exception, args);
    }
    #endregion

    #region Helper Methods
    private static void LogColored<T>(ILogger<T> logger, string message, ConsoleColor color, LogLevel logLevel, EventId? eventId = null, Exception? exception = null, params object[] args)
    {
        if (UseColoring)
        {
            Console.ForegroundColor = color;

            // Write crit/warn/etc, possibly in a different foreground color.
            WriteShortLogLevel(logLevel);
            var categoryName = typeof(T).FullName;
            Console.WriteLine($": {categoryName}[{eventId ?? 0}]");
            Console.WriteLine($"      {message}");
            if (exception != null)
            {
                Console.WriteLine($"      {exception.GetType().FullName}: {exception.Message}");
            }
            Console.ResetColor();
            return;
        }

        switch (logLevel)
        {
            case LogLevel.Trace:
                logger.LogTrace(eventId ?? default, exception, message, args);
                break;
            case LogLevel.Debug:
                logger.LogDebug(eventId ?? default, exception, message, args);
                break;
            case LogLevel.Information:
                logger.LogInformation(eventId ?? default, exception, message, args);
                break;
            case LogLevel.Warning:
                logger.LogWarning(eventId ?? default, exception, message, args);
                break;
            case LogLevel.Error:
                logger.LogError(eventId ?? default, exception, message, args);
                break;
            case LogLevel.Critical:
                logger.LogCritical(eventId ?? default, exception, message, args);
                break;
        }
    }

    private static void WriteShortLogLevel(LogLevel logLevel)
    {
        var shortLogLevel = GetShortLogLevel(logLevel);
        if (logLevel == LogLevel.Error)
        {
            var saveFGColor = Console.ForegroundColor;
            var saveBGColor = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write($"{shortLogLevel}");
            Console.ForegroundColor = saveFGColor;
            Console.BackgroundColor = saveBGColor;
        }
        else if (logLevel == LogLevel.Critical)
        {
            var saveFGColor = Console.ForegroundColor;
            var saveBGColor = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write($"{shortLogLevel}");
            Console.ForegroundColor = saveFGColor;
            Console.BackgroundColor = saveBGColor;
        }
        else
        {
            Console.Write($"{shortLogLevel}");
        }
    }

    private static string GetShortLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => "unkn"
        };
    }
    #endregion
}
#endregion

public static class ConsoleExtensions
{
    /// <summary>
    /// Writes green text to the console.
    /// </summary>
    /// <param name="text">The text to write.</param>
    [DebuggerStepThrough]
    public static void ConsoleGreen(this string text)
    {
        text.ColoredWriteLine(ConsoleColor.Green);
    }

    /// <summary>
    /// Writes red text to the console.
    /// </summary>
    /// <param name="text">The text to write.</param>
    [DebuggerStepThrough]
    public static void ConsoleRed(this string text)
    {
        text.ColoredWriteLine(ConsoleColor.Red);
    }

    /// <summary>
    /// Writes yellow text to the console.
    /// </summary>
    /// <param name="text">The text to write.</param>
    [DebuggerStepThrough]
    public static void ConsoleYellow(this string text)
    {
        text.ColoredWriteLine(ConsoleColor.Yellow);
    }

    /// <summary>
    /// Writes out text with the specified ConsoleColor.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="color">The color to use when writing the text.</param>
    [DebuggerStepThrough]
    public static void ColoredWriteLine(this string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
