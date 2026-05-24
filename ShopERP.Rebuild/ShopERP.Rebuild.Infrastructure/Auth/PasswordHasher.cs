using System.Security.Cryptography;

namespace ShopERP.Rebuild.Infrastructure.Auth;

public static class PasswordHasher
{
    public static (string Hash, string Salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA512, 32);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool VerifyPassword(string password, string hashBase64, string saltBase64)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA512, 32);
        var expected = Convert.FromBase64String(hashBase64);
        return CryptographicOperations.FixedTimeEquals(computed, expected);
    }
}
