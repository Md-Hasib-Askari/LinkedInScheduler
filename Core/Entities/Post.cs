namespace LinkedInScheduler.Core.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
}