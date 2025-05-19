using System;

public class Login
{
    private const string CorrectUsername = "Admin";
    private const string CorrectPassword = "pass";
    private const int MaxAttempts = 3;
    private const int MinUsernameLength = 3;
    private const int MinPasswordLength = 3;

    
    public static string GetUsername(int attemptNumber)
    {
        string? username;
        bool isValid = false;

        do
        {
            Console.Write($"Attempt {attemptNumber}: \nPlease enter your username (minimum {MinUsernameLength} characters): ");
            username = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("Username cannot be empty or just whitespace. Please try again.");
            }
            else if (username.Length < MinUsernameLength)
            {
                Console.WriteLine($"Username must be at least {MinUsernameLength} characters long. Please try again.");
            }
            else
            {
                isValid = true;
            }
        } while (!isValid);

        return username!;
    }


    public static string GetPassword()
    {
        string? password;
        bool isValid = false;

        do
        {
            Console.Write($"Please enter your password (minimum {MinPasswordLength} characters): ");
            password = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Password cannot be empty or just whitespace. Please try again.");
            }
            else if (password.Length < MinPasswordLength)
            {
                Console.WriteLine($"Password must be at least {MinPasswordLength} characters long. Please try again.");
            }
            else
            {
                isValid = true;
            }
        } while (!isValid);

        return password!;
    }

    
    public static bool ValidateCredentials(string username, string password)
    {
        return username == CorrectUsername && password == CorrectPassword;
    }


    public static void DisplayLoginSuccess()
    {
        Console.WriteLine("Login successful!");
    }

    public static void DisplayLoginFailure(int attemptsRemaining)
    {
        Console.WriteLine($"Invalid credentials. You have {attemptsRemaining} attempts remaining.");
    }

    public static void DisplayExceededAttempts()
    {
        Console.WriteLine($"Invalid attempts for {MaxAttempts} times. Exiting....");
    }

    public static void Main(string[] args)
    {
        int attemptsLeft = MaxAttempts;
        int currentAttempt = 1;

        while (attemptsLeft > 0)
        {
            string username = GetUsername(currentAttempt);
            string password = GetPassword();

            if (ValidateCredentials(username, password))
            {
                Console.Clear();
                DisplayLoginSuccess();
                return;
            }
            else
            {
                attemptsLeft--;
                if (attemptsLeft > 0)
                {
                    DisplayLoginFailure(attemptsLeft);
                }
            }
            currentAttempt++;
            Console.WriteLine();
        }

        DisplayExceededAttempts();
    }
}