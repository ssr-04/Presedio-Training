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

    public static List<int> Mergelist(List<int> list1, List<int> list2)
    {
        
        List<int> mergedList = new();

        foreach(int number in list1)
        {
            mergedList.Add(number);
        }

        foreach(int number in list2)
        {
            mergedList.Add(number);
        }

        return mergedList;
    }

    public static void PrintResults(List<int> mergedList)
    {
        Console.Write("\nMerged List: ");
        foreach(int number in mergedList)
        {
            Console.Write($"{number} ");
        }

        Console.WriteLine();
    }


    public static void Main(string[] args)
    {
        Console.WriteLine("List 1:");
        List<int> list1 = Getlist();
        
        Console.WriteLine("\nList 2:");
        List<int> list2 = Getlist();

        List<int> mergedList = Mergelist(list1, list2);

        PrintResults(mergedList);

    }
}