using System;
using System.Collections.Generic;

public class SudokuRowValidator
{
    private const int RowLength = 9;
    private const int ColumnLength = 9;

    public static (bool, List<int>) ParseListToInt(string[] numbers)
    {
        List<int> SudokuRow = new();
        int parsedNumber;
        bool isValid;
        foreach (string element in numbers)
        {
            isValid = int.TryParse(element, out parsedNumber);
            if (!isValid)
            {
                Console.WriteLine($"Invalid input: '{element}' is not a valid integer, Try again.");
                return (false, SudokuRow);
            }
            SudokuRow.Add(parsedNumber);
        }

        return (true, SudokuRow);
    }

    public static List<int> GetSudokuRow()
    {
        while (true)
        {
            Console.WriteLine($"Enter the Sudoku row ({RowLength} numbers separated by spaces):");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Empty input. Try again.");
                continue;
            }

            string[] splitted = input.Split(' ');

            if (splitted.Length != RowLength)
            {
                Console.WriteLine($"Invalid input: Expected {RowLength} numbers, but got {splitted.Length}, Try again.");
                continue;
            }

            var (done, parsed) = ParseListToInt(splitted!);
            if (done)
            {
                return parsed;
            }
        }
    }

    public static bool IsValidSudokuLine(List<int> line)
    {
        HashSet<int> numbers = new();

        foreach (int num in line)
        {
            if (num < 1 || num > 9)
            {
                Console.WriteLine("\nInvalid: Must contain numbers from 1 to 9.");
                return false;
            }

            if (!numbers.Add(num))
            {
                Console.WriteLine("\nInvalid: Contains duplicate numbers.");
                return false;
            }
        }

        return true;
    }

    public static (bool, List<List<int>>) GetSudukoBoard()
    {
        List<List<int>> board = new();
        for (int i = 0; i < ColumnLength; i++)
        {
            Console.WriteLine($"\nRow {i + 1}:");
            List<int> row = GetSudokuRow();
            bool isValid = IsValidSudokuLine(row);
            if (isValid)
            {
                board.Add(row);
            }
            else
            {
                Console.WriteLine("Re-enter this row.");
                i--; 
            }
        }
        return (true, board);
    }

    public static bool validateColumns(List<List<int>> board)
    {
        for(int i=0; i<ColumnLength; i++)
        {
            List<int> Column = new();
            for(int j=0; j<RowLength; j++)
            {
                Column.Add(board[j][i]);
            }
            bool isValid = IsValidSudokuLine(Column);
            if(!isValid)
            {
                Console.WriteLine($"Issue found in column {i+1}");
                return false;
            }
        }

        return true;
    }

    public static bool ValidateSubgrids(List<List<int>> board)
    {
        
        for (int blockRow = 0; blockRow < 9; blockRow += 3)
        {
            for (int blockCol = 0; blockCol < 9; blockCol += 3)
            {
                var block = new List<int>(9);
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        block.Add(board[blockRow + i][blockCol + j]);

                if (!IsValidSudokuLine(block))
                {
                    Console.WriteLine($"\nInvalid: 3×3 block starting at row {blockRow + 1}, column {blockCol + 1} is invalid.");
                    return false;
                }
            }
        }
        return true;
    }


    public static void Main(string[] args)
    {
        var (isValid, board) = GetSudukoBoard();
        if (isValid)
        {
            Console.WriteLine("\nSudoku Board Collected Successfully!");
        }
        if(validateColumns(board) && ValidateSubgrids(board))
        {
            Console.WriteLine("\nIt's a Valid Sudoku Board");
        }
    }
}

/*
Valid One:
6 3 9 5 7 4 1 8 2
5 4 1 8 2 9 3 7 6
7 8 2 6 1 3 9 5 4
1 9 8 4 6 7 5 2 3
3 6 5 9 8 2 4 1 7
4 2 7 1 3 5 8 6 9
9 5 6 7 4 8 2 3 1
8 1 3 2 9 6 7 4 5
2 7 4 3 5 1 6 9 8
*/