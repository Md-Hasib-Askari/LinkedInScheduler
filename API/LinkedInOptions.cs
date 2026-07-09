namespace LinkedInScheduler.API;

/// <summary>
/// Bind this to the "LinkedIn" section of appsettings.json / user-secrets.
/// Never commit ClientSecret to source control, use dotnet user-secrets locally
/// and environment variables / a secret store in production.
/// </summary>
public class LinkedInOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    // Must exactly match a redirect URL registered in the LinkedIn Developer Portal.
    public string RedirectUri { get; set; } = "https://localhost:5001/linkedin/callback";

    // How often the background worker checks for due posts.
    public int PollIntervalSeconds { get; set; } = 60;
}
