using System;

public class NumberComparison
{
    public static double GetValidNumber(string prompt)
    {
        double number;
        bool isValid;

        do
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();

            isValid = double.TryParse(input, out number);

            if(!isValid)
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        } while(!isValid);

        return number;
    }

    public static double FindLargest(double num1, double num2)
    {
        double largest = num1>=num2 ? num1 : num2;
        return largest;
    }
    public static void Main(string[] args)
    {
        double num1 = GetValidNumber("Enter the first number: ");
        double num2 = GetValidNumber("Enter the second number: ");

        Console.WriteLine($"Largest among {num1} and {num2} is {FindLargest(num1, num2)}");
    }
}