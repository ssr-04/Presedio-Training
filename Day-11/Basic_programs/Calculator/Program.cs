using System;

public class Calculator
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

            if (!isValid)
            {
                Console.WriteLine("Invalid input. Please enter a valid number.");
            }
        } while (!isValid);

        return number;
    }

    public static char GetValidOperation()
    {
        char operation;

        do
        {
            Console.Write("Enter the operation you want to perform (+, -, *, /): ");
            string? input = Console.ReadLine();

            if (!string.IsNullOrEmpty(input) && input.Length == 1 && "+-*/".Contains(input))
            {
                operation = input[0];
                break;
            }
            else
            {
                Console.WriteLine("Invalid operation. Please enter +, -, *, or /.");
            }

        } while (true);

        return operation;
    }

    public static double PerformOperation(double num1, double num2, char operation)
    {
        switch (operation)
        {
            case '+':
                return num1 + num2;
            case '-':
                return num1 - num2;
            case '*':
                return num1 * num2;
            case '/':
                if (num2 == 0)
                {
                    Console.WriteLine("Error: Cannot divide by zero");
                    return double.NaN;
                }
                return num1 / num2;
            default:
                Console.WriteLine("Error: Invalid operation encountered.");
                return double.NaN;
        }
    }

    public static void Main(string[] args)
    {
        double num1 = GetValidNumber("Enter the first number: ");
        double num2 = GetValidNumber("Enter the second number: ");

        char operation = GetValidOperation();

        double result = PerformOperation(num1, num2, operation);

        if (!double.IsNaN(result))
        {
            Console.WriteLine($"{num1} {operation} {num2} = {result}");
        }
    }
}
