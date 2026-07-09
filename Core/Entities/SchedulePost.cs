using System.Text.Json.Serialization;

namespace LinkedInScheduler.Core.Entities;

[JsonConverter(typeof(JsonNumberEnumConverter<PostStatus>))]
public enum PostStatus
{
    Pending,
    Publishing,
    Published,
    Failed,
}

/// <summary>
/// A single LinkedIn post waiting to go out.
/// If you already store your posts somewhere in your existing app,
/// just add the ScheduledTimeUtc / Status / LinkedInPostUrn / ErrorMessage
/// columns to that table instead of using this one.
/// </summary>
public class ScheduledPost
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    // Store in UTC to avoid timezone bugs when the worker checks "is it due yet".
    public DateTime ScheduledTimeUtc { get; set; }

    public PostStatus Status { get; set; } = PostStatus.Pending;

    public DateTime? PublishedAtUtc { get; set; }

    // The LinkedIn post URN returned after a successful publish, e.g. urn:li:share:12345
    public string? LinkedInPostUrn { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; } = 0;
}
