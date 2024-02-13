using System;
using System.Security.Cryptography;
using System.Text;

namespace Debricked.Helpers
{
    internal static class EncryptionHelper
    {
        private static readonly byte[] entropy = { 24, 37, 8, 55, 16, 49 };
        public static string Encrypt(string text)
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = ProtectedData.Protect(plaintext, entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string text)
        {
            byte[] encryptedText = Convert.FromBase64String(text);
            byte[] plaintext = ProtectedData.Unprotect(encryptedText, entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plaintext);
        }
    }
}
