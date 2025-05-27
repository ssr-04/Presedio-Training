using FirstTwitterApp.Models;

namespace FirstTwitterApp.Interfaces.Repositories
{
    public interface ILikeRepository
    {
        Task<IEnumerable<Like>> GetAllLikesAsync();
        Task<Like?> GetLikeByIdAsync(int id);
        Task<Like?> GetLikeByUserAndTweetAsync(int userId, int tweetId);
        Task<IEnumerable<Like>> GetLikesByTweetIdAsync(int tweetId);
        Task<IEnumerable<Like>> GetLikesByUserIdAsync(int userId);
        Task<Like> AddLikeAsync(Like like);
        Task<bool> RemoveLikeAsync(int id);
        Task<bool> LikeExistsAsync(int userId, int tweetId);
    }
}