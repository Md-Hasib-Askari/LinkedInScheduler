namespace LinkedInScheduler.Core.Entities;

/// <summary>
/// Holds the OAuth tokens for the single LinkedIn account this tool posts as.
/// Since this is a personal tool (not multi-tenant), there's normally just one row.
/// </summary>
public class LinkedInToken
{
    public int Id { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresUtc { get; set; }

    // e.g. "urn:li:person:AbC123xyz" — fetched once via the userinfo endpoint after login.
    public string MemberUrn { get; set; } = string.Empty;
}
