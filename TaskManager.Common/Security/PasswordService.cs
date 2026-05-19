using System.Security.Cryptography;
using TaskManager.Common.Interfaces;

namespace TaskManager.Common.Security
{
    /// <summary>
    /// PBKDF2-HMAC-SHA256 password hashing.
    /// Stored format: {iterations}.{salt-base64}.{hash-base64}
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 600_000;
        private const char Delimiter = '.';
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

            return string.Join(Delimiter,
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash))
            {
                return false;
            }

            var parts = hash.Split(Delimiter);
            if (parts.Length != 3 || !int.TryParse(parts[0], out int iterations))
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expected = Convert.FromBase64String(parts[2]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
