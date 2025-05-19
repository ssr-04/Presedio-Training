using System;
using System.Text;

public class CaesarCipher
{
    private const int Shift = 3; 
    private const int AlphabetLength = 26;

    public static bool IsValidMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            Console.WriteLine("Invalid input: Message cannot be empty.");
            return false;
        }

        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] < 'a' || message[i] > 'z')
            {
                Console.WriteLine("Invalid input: Message must contain only lowercase letters (a-z).");
                return false;
            }
        }
        return true;
    }

    
    public static string Encrypt(string message)
    {
        if (!IsValidMessage(message))
        {
            return string.Empty;
        }

        StringBuilder encryptedMessage = new StringBuilder();
        foreach (char c in message)
        {
            int shiftedChar = c + Shift;
            if (shiftedChar > 'z')
            {
                shiftedChar -= AlphabetLength; 
            }
            encryptedMessage.Append((char)shiftedChar);
        }
        return encryptedMessage.ToString();
    }

    public static string Decrypt(string message)
    {
        if (!IsValidMessage(message))
        {
            return string.Empty;
        }

        StringBuilder decryptedMessage = new StringBuilder();
        foreach (char c in message)
        {
            int shiftedChar = c - Shift;
            if (shiftedChar < 'a')
            {
                shiftedChar += AlphabetLength;
            }
            decryptedMessage.Append((char)shiftedChar);
        }
        return decryptedMessage.ToString();
    }


    public static void DisplayMenu()
    {
        Console.WriteLine("\nCipher Menu:");
        Console.WriteLine("1. Encrypt Message");
        Console.WriteLine("2. Decrypt Message");
        Console.WriteLine("3. Exit");
        Console.Write("Enter your choice: ");
    }

    
    public static string GetMessage()
    {
        Console.Write("Enter your message (lowercase letters only): ");
        return Console.ReadLine() ?? "";
    }

    public static void Main(string[] args)
    {
        while (true)
        {
            DisplayMenu();
            string choice = Console.ReadLine() ?? ""; 

            switch (choice)
            {
                case "1":
                    string messageToEncrypt = GetMessage();
                    string encryptedMessage = Encrypt(messageToEncrypt);
                    if (encryptedMessage != string.Empty)
                    {
                        Console.WriteLine($"Encrypted message: {encryptedMessage}");
                    }
                    break;
                case "2":
                    string messageToDecrypt = GetMessage();
                    string decryptedMessage = Decrypt(messageToDecrypt);
                    if (decryptedMessage != string.Empty)
                    {
                        Console.WriteLine($"Decrypted message: {decryptedMessage}");
                    }
                    break;
                case "3":
                    Console.WriteLine("Exiting application.");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}

