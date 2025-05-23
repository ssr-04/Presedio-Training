// Target
public interface ILogger
{
    void LogInfo(string message);
    void LogError(string message, Exception? ex = null);
}

// Adaptee (needed to convert)
// This is some legacy logging stuff
public class LegacyLogger
{
    public void WriteEntry(string level, string text)
    {
        Console.WriteLine($"[LEGACY_LOG]: {level.ToUpper()} - {text}");
    }

    public void ReportFailure(string errorMessage, int errorCode)
    {
        Console.WriteLine($"[LEGACY_ERROR_REPORT]: Code {errorCode} - {errorMessage}");
    }
}


// Adapter
public class LegacyLoggerAdapter : ILogger
{
    private readonly LegacyLogger _legacyLogger;

    public LegacyLoggerAdapter(LegacyLogger legacyLogger)
    {
        _legacyLogger = legacyLogger ?? throw new ArgumentNullException(nameof(legacyLogger));
        Console.WriteLine("[Adapter]: LegacyLoggerAdapter initialized.");
    }

    // Implements log info using WriteEntry
    public void LogInfo(string message)
    {
        _legacyLogger.WriteEntry("INFO", message);
    }

    // Implements log error using report failure
    public void LogError(string message, Exception? ex = null)
    {
        string fullMessage = ex != null ? $"{message} (Exception: {ex.GetType().Name} - {ex.Message})" : message;
        _legacyLogger.ReportFailure(fullMessage, 500); // generic error code 
    }
}

//Main
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Adapter Design Pattern Example ---");

        Console.WriteLine("\n--- Using Legacy Logger via Adapter ---");
        LegacyLogger oldLogger = new LegacyLogger(); // The incompatible legacy one
        ILogger adapterLogger = new LegacyLoggerAdapter(oldLogger); // The Adapted one

        // Client uses the Adapter just like any other ILogger
        adapterLogger.LogInfo("Application started.");
        adapterLogger.LogInfo("User 'someone' logged in.");
        try
        {
            throw new InvalidOperationException("Failed to process request.");
        }
        catch (Exception ex)
        {
            adapterLogger.LogError("An error occurred during operation.", ex);
        }

        Console.WriteLine("\nApplication Finished.");
    }
}

