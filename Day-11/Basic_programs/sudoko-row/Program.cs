using System;
using System.Collections.Generic;
using System.Linq;

public class SudokuRowValidator
{
    private const int RowLength = 9;

    public static (bool, List<int>) ParseListToInt(string[] numbers)
    {
        List<int> SudokuRow = new();
        int parsedNumber;
        bool isValid;
        foreach(string element in numbers)
        {
            isValid = int.TryParse(element, out parsedNumber);
            if(!isValid)
            {
                Console.WriteLine($"Invalid input: '{element}' is not a valid integer, Try again.");
                return (false, SudokuRow);
            }
            SudokuRow.Add(parsedNumber);
        }

        return(true, SudokuRow);
    }

    public static List<int> GetSudokuRow()
    {
        do
        {
            Console.WriteLine($"\nEnter the Sudoku row ({RowLength} numbers separated by spaces):");
            string? input = Console.ReadLine();

            string[] splitted = input.Split(' ');

            if(splitted.Length != RowLength)
            {
                Console.WriteLine($"Invalid input: Expected {RowLength} numbers, but got {splitted.Length}, Try again.");
                continue;
            }
            bool done;
            List<int> parsed;
            (done, parsed)  = ParseListToInt(splitted!);
            if(done)
            {
                return parsed;
            }
        }while(true);
    }

    public static bool IsValidSudokuRow(List<int> row)
    {
        HashSet<int> numbers = new HashSet<int>();

        for (int i = 0; i < RowLength; i++)
        {
            if (row[i] < 1 || row[i] > 9)
            {
                Console.WriteLine("\nInvalid: Row must contain numbers from 1 to 9.");
                return false;
            }

            if (numbers.Contains(row[i]))
            {
                Console.WriteLine("\nInvalid: Row contains duplicate numbers.");
                return false;
            }

            numbers.Add(row[i]);
        }

        return true;
    }

    public static void Main(String[] args)
    {
        List<int> row = GetSudokuRow();
        bool isValid = IsValidSudokuRow(row);
        if(isValid)
        {
            System.Console.WriteLine("The entered row is a valid sudoku row");
        }
    }
}