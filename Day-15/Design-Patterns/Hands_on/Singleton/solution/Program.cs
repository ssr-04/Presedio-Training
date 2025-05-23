namespace solution
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("File Access (Singleton - Eager)");

            IFileHandler fileHandler = FileHandler.Instance;

            var logWriter = new LogWriterService(fileHandler);
            var reportReader = new ReportReaderService(fileHandler);

            logWriter.LogUserAction("Admin", "Initialized application.");
            logWriter.LogUserAction("User1", "Clicked 'Login' button.");
            logWriter.LogUserAction("Admin", "Performed system backup.");

            reportReader.PrintAllLogs();

            logWriter.LogUserAction("System", "Application finished normal operations.");

        }
    }
}

