public sealed class ThreadSafeLogger
{
    private static ThreadSafeLogger? _instance;
    private static readonly object _lock = new object(); // A private static object for locking

    private ThreadSafeLogger()
    {
        Console.WriteLine("ThreadSafeLogger instance created.");
    }

    public static ThreadSafeLogger Instance
    {
        get
        {
            // First check: no instance already exists
            if (_instance == null)
            {
                // Acquire lock to ensure only one thread creates the instance
                lock (_lock)
                {
                    // passed the first check but waited for the lock.During which another thread might have created.
                    if (_instance == null)
                    {
                        _instance = new ThreadSafeLogger();
                    }
                }
            }
            return _instance;
        }
    }

    public void LogMessage(string message)
    {
        Console.WriteLine($"Log [ThreadSafeLock]: {message} ({DateTime.Now})");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("\n--- Thread-Safe Singleton with Lock Example ---");

        // Simulate concurrent access to highlight thread safety
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 5; i++)
        {
            int taskId = i;
            tasks.Add(Task.Run(() =>
            {
                ThreadSafeLogger logger = ThreadSafeLogger.Instance;
                logger.LogMessage($"Message from Task {taskId}");
            }));
        }

        Task.WaitAll(tasks.ToArray()); // Converting to array, because WaitAll needs array and not list
    }
}
