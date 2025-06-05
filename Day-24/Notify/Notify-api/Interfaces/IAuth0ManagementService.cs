using System.Threading.Tasks;

public interface IAuth0ManagementService
{
    Task<Auth0.ManagementApi.Models.User> CreateAuth0UserAsync(string email, string password, string role);
    Task<Auth0.ManagementApi.Models.User?> GetAuth0UserByEmailAsync(string email);
}