using System.Security.Cryptography;
using System.Text;

namespace JwtPratik.Services
{
    public static class PasswordHasher
    {
        public static (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var saltBase64 = Convert.ToBase64String(saltBytes);
            var hash = Hash(password, saltBase64);
            return (hash, saltBase64);
        }

        public static bool Verify(string password, string hash, string salt)
        {
            var computed = Hash(password, salt);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computed),
                Encoding.UTF8.GetBytes(hash)
            );
        }

        private static string Hash(string password, string saltBase64)
        {
            var saltBytes = Convert.FromBase64String(saltBase64);
            var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(32);
            return Convert.ToBase64String(key);
        }
    }
}
