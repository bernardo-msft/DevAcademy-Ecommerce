using Ecommerce.Application.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Application.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
    private const char Delimiter = ';';

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            _hashAlgorithmName,
            KeySize);

        return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    public bool VerifyPassword(string hashedPasswordWithSalt, string providedPassword)
    {
        var parts = hashedPasswordWithSalt.Split(Delimiter);
        if (parts.Length != 2)
        {
            // Or throw an exception, log, etc.
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);

        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(providedPassword),
            salt,
            Iterations,
            _hashAlgorithmName,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
    }
}