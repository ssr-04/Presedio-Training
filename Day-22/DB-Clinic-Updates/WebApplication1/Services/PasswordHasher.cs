using System.Text;
using BCrypt.Net;

public class PasswordHasher : IPasswordHasher
{
    public byte[] HashPassword(string password)
    {
        string hashedString = BCrypt.Net.BCrypt.HashPassword(password);
        return Encoding.UTF8.GetBytes(hashedString); // Convert the resulting string hash to byte[]
    }

    public bool VerifyPassword(string password, byte[] hashedPassword)
    {
        // Converting the stored byte[] hash back to string for BCrypt.Net.Verify
        string hashedPasswordString = Encoding.UTF8.GetString(hashedPassword);
        return BCrypt.Net.BCrypt.Verify(password, hashedPasswordString);
    }
}
