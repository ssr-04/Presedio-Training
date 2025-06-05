using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;

public class Auth0ManagementService : IAuth0ManagementService
{
    private readonly ManagementApiClient _managementApiClient;
    private readonly string _auth0Domain;
    private readonly string _managementClientId;
    private readonly string _managementClientSecret;

    public Auth0ManagementService(IConfiguration configuration)
    {
        _auth0Domain = configuration["Auth0:Domain"] ?? throw new ArgumentNullException("Auth0:Domain not found in config.");
        _managementClientId = configuration["Auth0:ManagementApi:ClientId"] ?? throw new ArgumentNullException("Auth0:ManagementApi:ClientId not found in config.");
        _managementClientSecret = configuration["Auth0:ManagementApi:ClientSecret"] ?? throw new ArgumentNullException("Auth0:ManagementApi:ClientSecret not found in config.");

        // Get an access token for the Management API
        var managementTokenClient = new AuthenticationApiClient(new Uri($"https://{_auth0Domain}/"));
        var tokenRequest = new ClientCredentialsTokenRequest
        {
            ClientId = _managementClientId,
            ClientSecret = _managementClientSecret,
            Audience = $"https://{_auth0Domain}/api/v2/" // This is the fixed audience for Auth0 Management API
        };
        var managementTokenResponse = managementTokenClient.GetTokenAsync(tokenRequest).Result; // Sync call for simplicity, consider async
        var managementAccessToken = managementTokenResponse.AccessToken;
        //System.Console.WriteLine(managementAccessToken);
        _managementApiClient = new ManagementApiClient(managementAccessToken, new Uri($"https://{_auth0Domain}/api/v2/"));
    }

    public async Task<Auth0.ManagementApi.Models.User> CreateAuth0UserAsync(string email, string password, string role)
    {
        var userCreateRequest = new UserCreateRequest
        {
            Email = email,
            Password = password,
            Connection = "Username-Password-Authentication", // The default database connection
            EmailVerified = false, // Set to true if you handle verification externally
            AppMetadata = new Dictionary<string, object>
            {
                { "role", role } // Store role in app_metadata
            }
        };

        var createdUser = await _managementApiClient.Users.CreateAsync(userCreateRequest);

        return createdUser;
    }


    public async Task<Auth0.ManagementApi.Models.User?> GetAuth0UserByEmailAsync(string email)
    {
        // Auth0's search syntax for users
        var users = await _managementApiClient.Users.GetAllAsync(new GetUsersRequest {
            SearchEngine = "v3", // Use v3 search engine for advanced queries
            Query = $"email:\"{email}\""
        });
        return users.FirstOrDefault();
    }
}
