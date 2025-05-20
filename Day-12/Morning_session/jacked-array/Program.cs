using System;

public class InstagramPost
{
    public string Caption { get; }
    public int Likes { get; }

    public InstagramPost(string caption, int likes)
    {
        Caption = caption;
        Likes = likes;
    }

    public override string ToString()
    {
        return $"Caption: {Caption} | Likes: {Likes}";
    }
}

public class InstagramApp
{
    private InstagramPost[][]? userPosts; //Jagged array to store posts of each user
    private int numberOfUsers;

    public static int GetValidInt(string prompt)
    {
        bool isValid;
        int number;
        do
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            isValid = int.TryParse(input, out number);
            if (!isValid)
            {
                Console.WriteLine("Invalid input, Please enter a whole number.\n");
            }
        } while (!isValid);
        return number;
    }

    public void Initialise()
    {
        while (true)
        {
            numberOfUsers = GetValidInt("Enter number of users: ");
            if (numberOfUsers <= 0) 
            {
                Console.WriteLine("Please enter valid number of users (greater than zero).\n");
            }
            else
            {
                userPosts = new InstagramPost[numberOfUsers][];
                break;
            }
        }
    }

    public (string, int) GetPostData(int post)
    {
        string? caption;
        while(true)
        {
            Console.Write($"Enter caption for post {post}: ");
            caption = Console.ReadLine();
            if(!string.IsNullOrWhiteSpace(caption) && caption.Length > 5)
            {
                break;
            }
            else
            {
                Console.WriteLine("--Enter a valid caption (greater than 5 characters).--");
            }
        }
         
        int likes;
        while (true)
        {
            likes = GetValidInt("Enter Number of Likes: ");
            if (likes < 0)
            {
                Console.WriteLine("--Enter valid number of Likes (a non-negative number).--"); 
            }
            else
            {
                return (caption!, likes!);
            }
        }
    }
    public void AddPostsToUsers()
    {
        for (int i = 0; i < numberOfUsers; i++)
        {
            Console.WriteLine($"\nUser {i + 1}:");
            int numberOfPosts = GetValidInt("How many Posts: ");
            while (numberOfPosts <= 0)
            {
                Console.WriteLine("Please enter valid number of posts (greater than zero).\n");
                numberOfPosts = GetValidInt("How many Posts?: ");
            }
            userPosts![i] = new InstagramPost[numberOfPosts];

            for (int j = 0; j < numberOfPosts; j++)
            {
                (string caption, int likes) = GetPostData(j + 1);
                userPosts[i][j] = new InstagramPost(caption, likes);
                Console.WriteLine();
            }
        }
    }

    public void ViewAllUsersAndPosts()
    {
        if (userPosts == null)
        {
            Console.WriteLine("No users or posts have been entered.");
            return;
        }

        Console.WriteLine("\n--- Displaying Instagram Posts ---");
        for (int i = 0; i < numberOfUsers; i++)
        {
            Console.WriteLine($"\nUser {i + 1}:");
            if (userPosts[i] == null || userPosts[i].Length == 0)
            {
                Console.WriteLine("  No posts available for this user.");
            }
            else
            {
                for (int j = 0; j < userPosts[i].Length; j++)
                {
                    Console.WriteLine($"  Post {j + 1} - {userPosts[i][j]}");
                }
            }
        }
    }

    public void Run()
    {
        Initialise();
        AddPostsToUsers();
        ViewAllUsersAndPosts();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        InstagramApp app = new InstagramApp();
        app.Run();
    }
}
