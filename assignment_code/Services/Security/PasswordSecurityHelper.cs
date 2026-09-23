using System;
using System.Security.Cryptography;
using System.Text;

namespace assignment_code.Services.Security
{
    public static class PasswordSecurityHelper
    {
        private const int SaltSize = 16; // 128-bit salt
        private const int HashSize = 32; // 256-bit derived key
        private const int Iterations = 10000;
        private const string HashPrefix = "PBKDF2";

        /// <summary>
        /// Hashes a plain-text password using PBKDF2 (HMAC-SHA256) with a cryptographic salt.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            return $"{HashPrefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Checks whether a stored password string is already hashed with PBKDF2.
        /// </summary>
        public static bool IsHashed(string storedHash)
        {
            return !string.IsNullOrEmpty(storedHash) && storedHash.StartsWith(HashPrefix + "$", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies a plain-text password against a stored hash or legacy password.
        /// Outputs needsRehash = true if the stored hash was a legacy plain text password that needs upgrading.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            // 1. Check for PBKDF2 secure hash format
            if (storedHash.StartsWith(HashPrefix + "$", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string[] parts = storedHash.Split('$');
                    if (parts.Length == 4)
                    {
                        int iterations = int.Parse(parts[1]);
                        byte[] salt = Convert.FromBase64String(parts[2]);
                        byte[] expectedHash = Convert.FromBase64String(parts[3]);

                        byte[] actualHash;
                        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                        {
                            actualHash = pbkdf2.GetBytes(expectedHash.Length);
                        }

                        return FixedTimeEquals(actualHash, expectedHash);
                    }
                }
                catch
                {
                    return false;
                }
            }

            // 2. Legacy plain text comparison (auto-upgrade trigger)
            if (string.Equals(password, storedHash, StringComparison.Ordinal))
            {
                needsRehash = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Constant-time comparison to prevent timing attacks.
        /// </summary>
        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}
