// Responsibility 1: User Data Model (Pure data structure)
public class UserData
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Only stores and not generates it within

    public UserData(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
    }
}

// Responsibility 2: User Validation
public class UserValidator
{
    public bool IsValid(UserData user)
    {
        if (string.IsNullOrWhiteSpace(user.Username) || user.Username.Length < 3)
        {
            Console.WriteLine("Validation Error: Username must be at least 3 characters.");
            return false;
        }
        if (!user.Email.Contains("@") || !user.Email.Contains("."))
        {
            Console.WriteLine("Validation Error: Invalid email format.");
            return false;
        }
        return true;
    }
}

// Responsibility 3: Database usage
public class UserRepository 
{
    public void SaveUser(UserData user)
    {
        // Simulating save
        Console.WriteLine($"Saving user {user.Username} to database...");
    }

    public UserData GetUser(string username)
    {
        // Simulate database retrieval
        Console.WriteLine($"Retrieving user {username} from database...");
        return new UserData(username, $"{username}@example.com", "hashedpassword123");
    }
}

// Responsibility 4: Password Hashing
public class PasswordHasher
{
    public string HashPassword(string plainPassword)
    {
        Console.WriteLine("Hashing password...");
        return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plainPassword)));
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        // simplified verification
        return HashPassword(plainPassword) == hashedPassword;
    }
}

// Responsibility 5: User Presentation/Reporting
public class UserReporter
{
    public void PrintUserInfo(UserData user)
    {
        Console.WriteLine($"\n--- User Info ---");
        Console.WriteLine($"Username: {user.Username}");
        Console.WriteLine($"Email: {user.Email}");
        Console.WriteLine($"Password Hash: {user.PasswordHash.Substring(0,10)}...");
        Console.WriteLine($"-----------------\n");
    }
}

// Putting togther
public class UserCreator
{
    private readonly UserValidator _validator;
    private readonly UserRepository _repository;
    private readonly PasswordHasher _hasher;

    public UserCreator(UserValidator validator, UserRepository repository, PasswordHasher hasher)
    {
        _validator = validator;
        _repository = repository;
        _hasher = hasher;
    }

    public bool CreateUser(string username, string email, string password)
    {
        string passwordHash = _hasher.HashPassword(password);
        UserData newUser = new UserData(username, email, passwordHash);

        if (_validator.IsValid(newUser))
        {
            _repository.SaveUser(newUser);
            Console.WriteLine($"User '{username}' created successfully.");
            return true;
        }
        else
        {
            Console.WriteLine($"User creation failed for '{username}'.");
            return false;
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        // Instantialization
        var validator = new UserValidator();
        var repository = new UserRepository();
        var hasher = new PasswordHasher();
        var reporter = new UserReporter();

        // Manager
        var userCreator = new UserCreator(validator, repository, hasher);

        bool success = userCreator.CreateUser("harrypotter", "harry@witch.com", "Lumous");

        if (success)
        {
            UserData retrievedUser = repository.GetUser("harrypotter");
            reporter.PrintUserInfo(retrievedUser);
        }
    }
}

/*
Why good?

- Increased readability
- Easier to test individual classes seperately
- Improved maintainance
- Reduced coupling
- Increased reusability
*/