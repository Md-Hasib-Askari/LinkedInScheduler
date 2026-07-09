using LinkedInScheduler.Core.Entities;

namespace LinkedInScheduler.Core.Services;

public interface IPostService
{
    Task<Post?> GetByIdAsync(Guid id);
    Task<IEnumerable<Post>> GetAllAsync();
    Task<Post> SchedulePostAsync(string content, DateTime scheduledAt);
    Task PublishPostAsync(Guid id);
    Task DeletePostAsync(Guid id);
}