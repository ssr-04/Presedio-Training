using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

public class BullsAndCows
{
    private const string SecretWord = "GAME"; 
    private const int WordLength = 4;

    public static bool IsValidGuess(string guess)
    {
        if (string.IsNullOrWhiteSpace(guess))
        {
            Console.WriteLine("Invalid input: Guess cannot be empty.");
            return false;
        }

        if (guess.Length != WordLength)
        {
            Console.WriteLine($"Invalid input: Guess must be {WordLength} letters long.");
            return false;
        }

        if (!guess.All(char.IsLetter))
        {
            Console.WriteLine("Invalid input: Guess must contain only letters.");
            return false;
        }

        return true;
    }

    
    public static (int bulls, int cows, string bullLetters, string cowLetters) CalculateBullsAndCows(string secretWord, string guess)
    {
        int bulls = 0;
        int cows = 0;
        StringBuilder bullLetters = new StringBuilder();
        StringBuilder cowLetters = new StringBuilder();

        string secret = secretWord.ToUpper();
        string guessed = guess.ToUpper();

        List<char> unmatchedSecret = new();
        List<char> unmatchedGuess = new();

        // find the bulls
        for (int i = 0; i < WordLength; i++)
        {
            if (secret[i] == guessed[i])
            {
                bulls++;
                bullLetters.Append(guessed[i]);
            }
            else
            {
                unmatchedGuess.Add(guessed[i]);
                unmatchedSecret.Add(secret[i]);
            }
        }

        // find the cows
        for (int i = 0; i < unmatchedGuess.Count; i++)
        {
            if (unmatchedSecret.Contains(unmatchedGuess[i]))
            {
                cows++;
                cowLetters.Append(unmatchedGuess[i]);
                unmatchedSecret.Remove(unmatchedGuess[i]);
            }
        }

        return (bulls, cows, bullLetters.ToString(), cowLetters.ToString());
    }


    public static void DisplayResult(int bulls, int cows, string bullLetters, string cowLetters)
    {
        Console.WriteLine($"{bulls} Bulls ({bullLetters}), {cows} Cows ({cowLetters})");
    }


    public static string GetUserGuess(int attempt)
    {
        string? guess; 
        Console.WriteLine($"\nAttempt-{attempt}");
        do
        {
            Console.Write($"Enter your {WordLength}-letter guess: ");
            guess = Console.ReadLine();
        } while (!IsValidGuess(guess ?? "")); 

        return guess!.ToUpper();
    }

    public static void Main(string[] args)
    {
        int attempts = 1;
        Console.WriteLine("Welcome to Bulls and Cows!");
        Console.WriteLine($"Try to guess the {WordLength}-letter secret word.");

        while (true)
        {
            string guess = GetUserGuess(attempts);

            (int bulls, int cows, string bullLetters, string cowLetters) = CalculateBullsAndCows(SecretWord, guess);
            DisplayResult(bulls, cows, bullLetters, cowLetters);

            if (bulls == WordLength)
            {
                Console.WriteLine($"\nCongratulations! You guessed the word in {attempts} attempts.");
                break;
            }
            attempts++;
        }
    }
}

