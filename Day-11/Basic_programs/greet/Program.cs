using System;

public class Program
{
    static void GreetUser(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannot be empty.");
        }
        else
        {
            Console.WriteLine($"Hello, {name}!");
        }
    }
    public static void Main(string[] args)
    {
        Console.Write("Please enter your name: ");
        string? name = Console.ReadLine();
        GreetUser(name);
    }
}