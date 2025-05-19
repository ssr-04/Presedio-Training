using System;
using System.Collections.Generic;

public class Divisible
{
    private const int NumberOfInputs = 10;
    private const int Divisor = 7;


    public static int GetValidInteger(string prompt)
    {
        int number;
        bool isValid;

        do
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            isValid = int.TryParse(input, out number);

            if (!isValid)
            {
                Console.WriteLine("Invalid input. Please enter a whole number.");
            }

        } while (!isValid);

        return number!;
    }

    
    public static bool IsDivisible(int number, int divisor)
    {
        return number % divisor == 0;
    }

    
    public static int CountDivisibleNumbers(List<int> numbers, int divisor)
    {
        int count = 0;
        foreach (int number in numbers)
        {
            if (IsDivisible(number, divisor))
            {
                count++;
            }
        }
        return count;
    }


    public static void Main(string[] args)
    {
        List<int> userNumbers = new List<int>();

        Console.WriteLine($"Please enter {NumberOfInputs} numbers:");

        for (int i = 0; i < NumberOfInputs; i++)
        {
            int number = GetValidInteger($"Enter number {i + 1}: ");
            userNumbers.Add(number);
        }

        int divisibleCount = CountDivisibleNumbers(userNumbers, Divisor);
        Console.WriteLine($"\nOut of the {NumberOfInputs} numbers you entered, {divisibleCount} are divisible by {Divisor}.");
    }
}