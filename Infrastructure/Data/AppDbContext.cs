using LinkedInScheduler.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LinkedInScheduler.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<ScheduledPost> ScheduledPosts => Set<ScheduledPost>();
    public DbSet<LinkedInToken> LinkedInTokens => Set<LinkedInToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(3000);
            entity.Property(e => e.ScheduledAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
