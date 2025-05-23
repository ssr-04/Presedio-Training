using solution.FileHandling;
using solution.Interfaces;
using solution.Models;

public class Program
{
    private static RealFile? _sensitiveFile; // The real file instance
    private static User? _loggedInUser;     // The user currently "logged in"

    public static void Main(string[] args)
    {
        Console.WriteLine("--- Secure File Access System ---");

        string sensitiveFilePath = "C:\\Users\\VC\\Documents\\Learning\\Internship-training\\Presedio-Training\\Day-15\\Design-Patterns\\Hands_on\\Proxy\\solution\\sensitive_data.txt";

        if (!File.Exists(sensitiveFilePath))
        {
            Console.Error.WriteLine($"Error: file '{sensitiveFilePath}' not found.");
            Console.ReadKey();
            return;
        }

        try
        {
            _sensitiveFile = new RealFile(sensitiveFilePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to initialize sensitive file: {ex.Message}");
            Console.ReadKey();
            return;
        }

        RunMenu();

        Console.WriteLine("\n--- Application Exited ---");
        Console.ReadKey();
    }

    private static void RunMenu()
    {
        bool exit = false;
        while (!exit)
        {
            DisplayMenu();
            string? choice = Console.ReadLine();

            switch (choice?.Trim())
            {
                case "1":
                    CreateUser();
                    break;
                case "2":
                    AttemptFileAccess();
                    break;
                case "3":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("\nInvalid choice. Please enter 1, 2, or 3.");
                    break;
            }
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private static void DisplayMenu()
    {
        Console.WriteLine("\n--- Main Menu ---");
        Console.WriteLine("1. Create/Switch User");
        Console.WriteLine("2. Attempt File Access (using current user)");
        Console.WriteLine("3. Exit");
        Console.WriteLine("-----------------");
        if (_loggedInUser != null)
        {
            Console.WriteLine($"Current User: {_loggedInUser.Username} (Role: {_loggedInUser.Role})");
        }
        else
        {
            Console.WriteLine("No user selected. Please create a user first.");
        }
        Console.Write("Enter your choice: ");
    }

    private static void CreateUser()
    {
        Console.WriteLine("\n--- Create New User ---");
        Console.Write("Enter username: ");
        string? username = Console.ReadLine();

        Console.WriteLine("Select Role:");
        Console.WriteLine("  1. Admin");
        Console.WriteLine("  2. User");
        Console.WriteLine("  3. Guest");
        Console.Write("Enter role number: ");
        string? roleChoice = Console.ReadLine();

        UserRole selectedRole = UserRole.Unknown;
        bool roleParsed = false;

        if (int.TryParse(roleChoice, out int roleNum))
        {
            switch (roleNum)
            {
                case 1: selectedRole = UserRole.Admin; roleParsed = true; break;
                case 2: selectedRole = UserRole.User; roleParsed = true; break;
                case 3: selectedRole = UserRole.Guest; roleParsed = true; break;
                default: Console.WriteLine("Invalid role number."); break;
            }
        }
        else
        {
            Console.WriteLine("Invalid input for role. Please enter a number.");
        }

        if (roleParsed && !string.IsNullOrWhiteSpace(username))
        {
            try
            {
                _loggedInUser = new User(username, selectedRole);
                Console.WriteLine($"\nUser '{_loggedInUser.Username}' with role '{_loggedInUser.Role}' created and selected.");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
            }
        }
        else if (string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("Username cannot be empty.");
        }
    }

    private static void AttemptFileAccess()
    {
        if (_loggedInUser == null)
        {
            Console.WriteLine("\nNo user is currently selected. Please create a user first (Option 1).");
            return;
        }

        if (_sensitiveFile == null)
        {
            Console.WriteLine("\nError: Sensitive file not initialized. Cannot attempt access.");
            return;
        }

        Console.WriteLine($"\n--- Attempting File Access for {_loggedInUser.Username} ---");
        IFile fileProxy = new ProxyFile(_sensitiveFile, _loggedInUser);
        string content = fileProxy.Read();
        Console.WriteLine("\n--- File Content / Access Result ---");
        Console.WriteLine(content);
        Console.WriteLine("------------------------------------");
    }
}
