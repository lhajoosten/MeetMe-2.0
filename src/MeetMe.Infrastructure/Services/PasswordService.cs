using MeetMe.Application.Services;
using System.Security.Cryptography;

namespace MeetMe.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 128 / 8; // 128 bits
    private const int KeySize = 256 / 8; // 256 bits
    private const int Iterations = 10000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public string HashPassword(string password)
    {
        using var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithm);
        var key = Convert.ToBase64String(algorithm.GetBytes(KeySize));
        var salt = Convert.ToBase64String(algorithm.Salt);

        return $"{Iterations}.{salt}.{key}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            var parts = hashedPassword.Split('.', 3);

            if (parts.Length != 3)
            {
                return false;
            }

            var iterations = Convert.ToInt32(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using var algorithm = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithm);
            var keyToCheck = algorithm.GetBytes(KeySize);

            return keyToCheck.SequenceEqual(key);
        }
        catch
        {
            return false;
        }
    }
}
