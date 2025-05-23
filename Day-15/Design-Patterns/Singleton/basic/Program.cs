// Basic Non-Thread safe
public sealed class Logger //Sealed prevents inheritence (which would introduce issues with singleton property)
{
    // 1) Private static instance
    private static Logger? _instance;

    // 2) Private constructor (to prevent external init)
    private Logger()
    {
        Console.WriteLine("Logger instance created.");
    }

    // 3) Public static property acts as provider
    public static Logger Instance
    {
        get
        {
            if (_instance == null) // If both threds check at same time, it will be null leading to 2 creations
            {
                _instance = new Logger();
            }
            return _instance;
        }
    }

    public void LogMessage(string message)
    {
        Console.WriteLine($"Log: {message} ({DateTime.Now})");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Non-Thread-Safe Singleton Example ---");

        // Get the single instance
        Logger logger1 = Logger.Instance;
        logger1.LogMessage("Application started.");

        // Get the instance again
        Logger logger2 = Logger.Instance;
        logger2.LogMessage("User logged in.");

        // Verify that both references point to the same instance
        Console.WriteLine($"\nlogger1 and logger2 the same instance? {ReferenceEquals(logger1, logger2)}");

        // It's hard to demostrate multi-thread scenerio so skipping it, but conceptually
        // Task.Run(() => Logger.Instance);
        // Task.Run(() => Logger.Instance);

    }
}
