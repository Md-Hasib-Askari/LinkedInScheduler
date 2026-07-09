using LinkedInScheduler.Core.Entities;

namespace LinkedInScheduler.Core.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(Guid id);
    Task<IEnumerable<Post>> GetAllAsync();
    Task<IEnumerable<Post>> GetPendingAsync();
    Task<Post> AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Guid id);
}