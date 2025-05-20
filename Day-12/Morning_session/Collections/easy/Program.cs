using System;
using System.Collections.Generic;
using System.Linq;

public class Helper
{
    public static string GetValidEmployeeName(string prompt)
    {
        string? name;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(prompt);
            Console.ForegroundColor = ConsoleColor.Blue;
            name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name) && name.All(char.IsLetter))
            {
                Console.ResetColor();
                return name;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid: Please enter a valid name.\n");
                Console.ResetColor();
            }
        }
    }
}

public class EmployeePromotion
{
    private List<string> promotionList = new();

    public void GetPromotionListFromUser()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Please enter the employee names in the order of their eligibility for promotion (Press Enter on blank to stop)");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Blue;
        string? name;
        while (true)
        {
            name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                break;
            }
            else
            {
                if (name.All(char.IsLetter))
                {
                    promotionList.Add(name);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid: Please enter a valid name (only characters).");
                    Console.ResetColor();
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
            }
        }
        Console.ResetColor();
    }

    public void PrintPromotionInOrder()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n-- Displaying the employees in order of promotion (As entered by user) --");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Blue;
        foreach (string employee in promotionList)
        {
            Console.WriteLine(employee);
        }
        Console.ResetColor();
    }

    public void FindPromotionPosition()
    {
        string? employeeNameToCheck = Helper.GetValidEmployeeName("\nPlease enter the name of the employee to check promotion position");
        int index = promotionList.IndexOf(employeeNameToCheck);

        Console.ForegroundColor = ConsoleColor.Green;
        if (index != -1)
        {
            Console.WriteLine($"\"{employeeNameToCheck}\" is in position {index + 1} for promotion.");
        }
        else
        {
            Console.WriteLine($"\"{employeeNameToCheck}\" was not found in the promotion list.");
        }
        Console.ResetColor();
    }

    public void TrimExcessCapacity()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nThe current capacity of the collection is {promotionList.Capacity}");
        Console.ResetColor();

        promotionList.TrimExcess();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nThe capacity after removing extra space is {promotionList.Capacity}");
        Console.ResetColor();
    }

    public void PrintSortedPromotionList()
    {
        List<string> sortedList = new List<string>(promotionList); // Copying to preserve original
        sortedList.Sort();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nPromoted employee list (Sorted):");
        foreach (string name in sortedList)
        {
            Console.WriteLine(name);
        }
        Console.ResetColor();
    }
}

class Program
{
    public static void ShowMenu()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n1) Populating and displaying in order");
        Console.WriteLine("2) Populating and checking the promotion position of an employee");
        Console.WriteLine("3) Populating and trimming excess capacity");
        Console.WriteLine("4) Populating and printing employees in ascending order");
        Console.WriteLine("5) Exit");
        Console.ResetColor();
    }

    public static int GetOption()
    {
        int choice;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter your choice: ");
            if (int.TryParse(Console.ReadLine(), out choice))
            {
                Console.ResetColor();
                return choice;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid: Enter a valid number as choice.");
                Console.ResetColor();
            }
        }
    }

    public static void Main(string[] args)
    {
        EmployeePromotion EPInstance = new();

        while (true)
        {
            ShowMenu();
            int choice = GetOption();

            switch (choice)
            {
                case 1:
                    EmployeePromotion EPInstance = new();
                    EPInstance.GetPromotionListFromUser();
                    EPInstance.PrintPromotionInOrder();
                    break;

                case 2:
                    EmployeePromotion EPInstance = new();
                    EPInstance.GetPromotionListFromUser();
                    EPInstance.FindPromotionPosition();
                    break;

                case 3:
                    EmployeePromotion EPInstance = new();
                    EPInstance.GetPromotionListFromUser();
                    EPInstance.TrimExcessCapacity();
                    break;

                case 4:
                    EmployeePromotion EPInstance = new();
                    EPInstance.GetPromotionListFromUser();
                    EPInstance.PrintSortedPromotionList();
                    break;

                case 5:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Exiting program. Thank you!");
                    Console.ResetColor();
                    Environment.Exit(0);
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ResetColor();
                    break;
            }
        }
    }
}
