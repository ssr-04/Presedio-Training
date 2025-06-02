
public interface IPasswordHasher
{
    byte[] HashPassword(string password);
    bool VerifyPassword(string password, byte[] hashedPassword);
}