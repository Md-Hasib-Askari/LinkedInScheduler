using System.Net.Http.Json;
using LinkedInScheduler.API;
using LinkedInScheduler.Core.Entities;
using LinkedInScheduler.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LinkedInScheduler.Core.Services;

public class LinkedInAuthService(
    HttpClient http,
    IOptions<LinkedInOptions> options,
    AppDbContext db
)
{
    private const string AuthorizeUrl = "https://www.linkedin.com/oauth/v2/authorization";
    private const string TokenUrl = "https://www.linkedin.com/oauth/v2/accessToken";
    private const string UserInfoUrl = "https://api.linkedin.com/v2/userinfo";

    private readonly HttpClient _http = http;
    private readonly LinkedInOptions _options = options.Value;
    private readonly AppDbContext _db = db;

    /// <summary>
    /// Step 1: send the user (you) here once to grant access.
    /// Scope "w_member_social" is what lets the app post on your behalf;
    /// "openid profile" lets us resolve your member URN afterwards.
    /// </summary>
    public string BuildAuthorizationUrl(string state)
    {
        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["state"] = state,
            ["scope"] = "openid profile w_member_social",
        };
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        return $"{AuthorizeUrl}?{qs}";
    }

    /// <summary>
    /// Step 2: LinkedIn redirects back to your callback with ?code=... — exchange it for tokens
    /// and store them. This only needs to happen once (then refresh handles the rest for a year).
    /// </summary>
    public async Task<LinkedInToken> ExchangeCodeForTokenAsync(
        string code,
        CancellationToken ct = default
    )
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        };

        var response = await _http.PostAsync(TokenUrl, new FormUrlEncodedContent(form), ct);
        response.EnsureSuccessStatusCode();
        var payload =
            await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Empty token response from LinkedIn.");

        var memberUrn = await FetchMemberUrnAsync(payload.access_token, ct);

        // Personal tool = single-row token store. Replace any existing row.
        var existing = await _db.LinkedInTokens.FirstOrDefaultAsync(ct);
        if (existing is null)
        {
            existing = new LinkedInToken();
            _db.LinkedInTokens.Add(existing);
        }

        existing.AccessToken = payload.access_token;
        existing.AccessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(payload.expires_in);
        existing.RefreshToken = payload.refresh_token ?? existing.RefreshToken;
        existing.RefreshTokenExpiresUtc = payload.refresh_token_expires_in.HasValue
            ? DateTime.UtcNow.AddSeconds(payload.refresh_token_expires_in.Value)
            : existing.RefreshTokenExpiresUtc;
        existing.MemberUrn = memberUrn;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    private async Task<string> FetchMemberUrnAsync(string accessToken, CancellationToken ct)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, UserInfoUrl);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            accessToken
        );
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var info =
            await response.Content.ReadFromJsonAsync<UserInfoResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Empty userinfo response from LinkedIn.");
        return $"urn:li:person:{info.sub}";
    }

    /// <summary>
    /// Call this before every publish. Refreshes the access token if it's close to expiring.
    /// Refresh tokens are valid ~365 days, so this only fails if the tool has been idle for a year.
    /// </summary>
    public async Task<LinkedInToken> GetValidTokenAsync(CancellationToken ct = default)
    {
        var token =
            await _db.LinkedInTokens.FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException(
                "No LinkedIn account connected yet — visit /linkedin/login first."
            );

        if (token.AccessTokenExpiresUtc > DateTime.UtcNow.AddMinutes(5))
            return token;

        if (token.RefreshTokenExpiresUtc <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "Refresh token expired — you need to reconnect via /linkedin/login."
            );

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = token.RefreshToken,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
        };

        var response = await _http.PostAsync(TokenUrl, new FormUrlEncodedContent(form), ct);
        response.EnsureSuccessStatusCode();
        var payload =
            await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Empty refresh response from LinkedIn.");

        token.AccessToken = payload.access_token;
        token.AccessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(payload.expires_in);
        if (payload.refresh_token is not null)
            token.RefreshToken = payload.refresh_token;
        if (payload.refresh_token_expires_in.HasValue)
            token.RefreshTokenExpiresUtc = DateTime.UtcNow.AddSeconds(
                payload.refresh_token_expires_in.Value
            );

        await _db.SaveChangesAsync(ct);
        return token;
    }

    private record TokenResponse(
        string access_token,
        int expires_in,
        string? refresh_token,
        int? refresh_token_expires_in
    );

    private record UserInfoResponse(string sub);
}
