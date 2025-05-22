public class User
{
    public string Username {get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }

    public User(string username, string email, string password)
    {
        Username = username;
        Email = email;
        PasswordHash = HashPassword(password); 
    }

    //Responsibilty 1: Validation
    public bool IsValid()
    {

        if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
        {
            Console.WriteLine("Validation Error: Username must be at least 3 characters.");
            return false;
        }
        if (!Email.Contains("@") || !Email.Contains("."))
        {
            Console.WriteLine("Validation Error: Invalid email format.");
            return false;
        }
        // Maybe more validations
        return true;
    }

    //Responsibilty 2: Managing data
    public void SaveToDatabase()
    {
        // Just simulating
        Console.WriteLine($"Saving user {Username} to database...");
    }

     // Responsibility 3: Hashing Passwords
    private string HashPassword(string password)
    {
        Console.WriteLine("Hashing password...");
        // some hashing algorithm logic
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password)));
    }

     // Responsibility 4: Reporting
    public void PrintUserInfo()
    {
        Console.WriteLine($"\n--- User Info ---");
        Console.WriteLine($"Username: {Username}");
        Console.WriteLine($"Email: {Email}");
        Console.WriteLine($"Password Hash: {PasswordHash.Substring(0,10)}..."); // truncating for display
        Console.WriteLine($"-----------------\n");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        User newUser = new User("Harry Potter", "Harry@witch.com", "Lumos");

        if (newUser.IsValid())
        {
            newUser.SaveToDatabase();
            newUser.PrintUserInfo();
        }
        else
        {
            Console.WriteLine("User creation failed due to validation errors.");
        }
    }
}

/*
Why it's bad?

- If we wanted to add DOB it's okay for a change as it's primary responsibility of User class
But..
- What if we need to change validation rules?
- What if change in saving to database logic?
- Change in hashing algo?
- Change in Printing

even tho they aren't directly related to User (unrelated concerns), it needs change here which is not a good practice

*/