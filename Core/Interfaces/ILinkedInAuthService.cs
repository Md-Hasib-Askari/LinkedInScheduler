using LinkedInScheduler.Core.Entities;

namespace LinkedInScheduler.Core.Interfaces;

public interface ILinkedInAuthService
{
    string BuildAuthorizationUrl(string state);
    Task<LinkedInToken> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
    Task<LinkedInToken> GetValidTokenAsync(CancellationToken ct = default);
}