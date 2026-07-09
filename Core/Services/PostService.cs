using LinkedInScheduler.Core.Entities;
using LinkedInScheduler.Core.Interfaces;

namespace LinkedInScheduler.Core.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repository;

    public PostService(IPostRepository repository)
    {
        _repository = repository;
    }

    public async Task<Post?> GetByIdAsync(Guid id) =>
        await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<Post>> GetAllAsync() =>
        await _repository.GetAllAsync();

    public async Task<Post> SchedulePostAsync(string content, DateTime scheduledAt)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = content,
            ScheduledAt = scheduledAt,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(post);
    }

    public async Task PublishPostAsync(Guid id)
    {
        var post = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Post with id {id} not found.");

        post.IsPublished = true;
        post.PublishedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(post);
    }

    public async Task DeletePostAsync(Guid id) =>
        await _repository.DeleteAsync(id);
}