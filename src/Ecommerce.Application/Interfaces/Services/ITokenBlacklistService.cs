namespace Ecommerce.Application.Interfaces.Services;

public interface ITokenBlacklistService
{
    /// <summary>
    /// Adds a token's JTI to the blacklist.
    /// </summary>
    /// <param name="jti">The JWT ID (JTI claim) of the token.</param>
    /// <param name="expiresAtUtc">The original expiry time of the token.</param>
    Task BlacklistTokenAsync(string jti, DateTimeOffset expiresAtUtc);

    /// <summary>
    /// Checks if a token's JTI is in the blacklist.
    /// </summary>
    /// <param name="jti">The JWT ID (JTI claim) of the token.</param>
    /// <returns>True if the token is blacklisted, false otherwise.</returns>
    Task<bool> IsTokenBlacklistedAsync(string jti);
}