using System;
using System.Collections.Generic;

public class RotatelistByOne
{
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

    public static List<int> Getlist()
    {
        List<int> list = new List<int>();
        int NumberOfElements;

        do
        {
            NumberOfElements = GetValidInteger("Enter the Size of the list: ");

            if(NumberOfElements < 1)
            {
                Console.WriteLine("The number of elements should be greater than zero.");
            }

        } while(NumberOfElements<1);
        
        Console.WriteLine($"\nPlease enter {NumberOfElements} numbers: ");

        for (int i = 0; i < NumberOfElements; i++)
        {
            int number = GetValidInteger($"Enter number {i + 1}: ");
            list.Add(number);
        }

        return list;
    }

    public static List<int> Rotatelist(List<int> list)
    {
        int firstElement = list[0];
        List<int> rotatedList = list.Skip(1).ToList();
        rotatedList.Add(firstElement);

        return rotatedList;
    }

    public static void PrintResults(List<int> original, List<int> rotated)
    {
        Console.Write("\nOriginal List: ");
        foreach(int number in original)
        {
            Console.Write($"{number} ");
        }

        Console.Write("\nRotated List: ");
        foreach(int number in rotated)
        {
            Console.Write($"{number} ");
        }
        Console.WriteLine();
    }


    public static void Main(string[] args)
    {
        List<int> originalList = Getlist();
        
        List<int> RotatedList = Rotatelist(originalList);

        PrintResults(originalList, RotatedList);

    }
}