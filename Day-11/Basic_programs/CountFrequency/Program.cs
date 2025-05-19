using System;
using System.Collections.Generic;

public class CountFrequencyOfInt
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

    public static Dictionary<int, int> CalculateFrequency(List<int> list)
    {
        Dictionary<int,int> frequencyMap = new();

        foreach (int element in list)
        {
            if (frequencyMap.ContainsKey(element))
            {
                frequencyMap[element]++;
            }
            else 
            {
                frequencyMap[element] = 1;
            }
        }

        return frequencyMap;
    }


    public static void PrintFrequency(Dictionary<int, int> frequencyMap)
    {
        Console.WriteLine();
        foreach (KeyValuePair<int,int> pair in frequencyMap)
        {
            Console.WriteLine($"{pair.Key} occurs {pair.Value} times");
        }
    }


    public static void Main(string[] args)
    {
        List<int> list = Getlist();
        
        Dictionary<int,int> frequencyMap = CalculateFrequency(list);

        PrintFrequency(frequencyMap);

    }
}