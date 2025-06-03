using System.Threading.Tasks;
using System.Security.Claims; // For JWT claims

public interface IAuthService
{
    Task<string?> LoginAsync(string username, string password);
    public List<Claim> GetUserClaims(User user);
}