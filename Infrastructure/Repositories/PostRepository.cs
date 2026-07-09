using LinkedInScheduler.Core.Entities;
using LinkedInScheduler.Core.Interfaces;
using LinkedInScheduler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LinkedInScheduler.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Post?> GetByIdAsync(Guid id) =>
        await _context.Posts.FindAsync(id);

    public async Task<IEnumerable<Post>> GetAllAsync() =>
        await _context.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<IEnumerable<Post>> GetPendingAsync() =>
        await _context.Posts
            .Where(p => !p.IsPublished && p.ScheduledAt <= DateTime.UtcNow)
            .ToListAsync();

    public async Task<Post> AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post is not null)
        {
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}