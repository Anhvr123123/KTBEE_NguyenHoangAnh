
using System;
using System.Security.Cryptography;

namespace KTBEE_NguyenHoangAnh.Models
{
    public static class PasswordHelper
    {
        private const int Iterations = 100000;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return "PBKDF2$" +
                    Iterations + "$" +
                    Convert.ToBase64String(salt) + "$" +
                    Convert.ToBase64String(hash);
            }
        }

        public static bool VerifyPassword(
            string password, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
                return false;

            if (!storedPassword.StartsWith("PBKDF2$"))
            {
                return string.Equals(
                    password,
                    storedPassword.TrimEnd(),
                    StringComparison.Ordinal);
            }

            try
            {
                string[] parts = storedPassword.Split('$');

                if (parts.Length != 4)
                    return false;

                int iterations = int.Parse(parts[1]);

                if (iterations < 10000 || iterations > 1000000)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations))
                {
                    byte[] actual = pbkdf2.GetBytes(expected.Length);

                    int difference = 0;

                    for (int i = 0; i < expected.Length; i++)
                    {
                        difference |= actual[i] ^ expected[i];
                    }

                    return difference == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
