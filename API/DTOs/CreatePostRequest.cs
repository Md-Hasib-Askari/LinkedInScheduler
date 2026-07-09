namespace LinkedInScheduler.API.DTOs;

public record CreatePostRequest(string Content, DateTime ScheduledAt);

public record PostResponse(Guid Id, string Content, DateTime ScheduledAt, bool IsPublished, DateTime CreatedAt, DateTime? PublishedAt);

public record UpdatePostRequest(string Content, DateTime ScheduledAt);