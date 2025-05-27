using FirstTwitterApp.Models;

namespace FirstTwitterApp.Interfaces.Repositories
{
    public interface ITweetRepository
    {
        Task<IEnumerable<Tweet>> GetAllTweetsAsync(bool includeDeleted = false);
        Task<Tweet?> GetTweetByIdAsync(int id, bool includeDeleted = false);
        Task<IEnumerable<Tweet>> GetTweetsByUserIdAsync(int userId, bool includeDeleted = false);
        Task<Tweet> AddTweetAsync(Tweet tweet);
        Task<Tweet?> UpdateTweetAsync(Tweet tweet);
        Task<bool> SoftDeleteTweetAsync(int id);
        Task<bool> TweetExistsAsync(int id, bool includeDeleted = false);
    }
}