using System;
using System.Security.Cryptography;
using System.Text;

namespace DB.Utilities
{
    /// <summary>
    /// Helper class for password hashing and verification using SHA256
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Hash a password using SHA256
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <returns>Base64 encoded hash</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            using (SHA256 sha256 = SHA256.Create())
            {
                // Compute hash from password bytes
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                // Convert byte array to Base64 string
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Verify a password against a stored hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <param name="storedHash">Stored hash to compare against</param>
        /// <returns>True if password matches, false otherwise</returns>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            // Hash the input password and compare with stored hash
            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(storedHash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Validate password strength (simple validation)
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Error message if invalid, null if valid</returns>
        public static string ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return "Password cannot be empty";

            if (password.Length < 6)
                return "Password must be at least 6 characters long";

            // Password is valid
            return null;
        }
    }
}
