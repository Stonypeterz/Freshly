using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Freshly.Identity
{
    public static class CustomExtensions
    {
        private static readonly string eKey = "J85a/gPN5mWh/jMQJPK!EWiMN5B@zWLU4TZn0fUSzJPRaw";
        private static readonly byte[] bt = new byte[16] { 0x99, 0xf6, 0x68, 0x6e, 0x50, 0xfd, 0x69, 0x64, 0x7a, 0x55, 0x64, 0x69, 0x77, 0xbd, 0xe6, 0xa9 };

        public static string ToHashed(this string data)
        {
            byte[] salt = new byte[16] { 0x59, 0x76, 0x68, 0x6e, 0x20, 0xfd, 0x65, 0x34, 0x70, 0x55, 0x64, 0x69, 0x77, 0xef, 0xc7, 0xff };
            var hashed = KeyDerivation.Pbkdf2(
                password: data,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 15000,
                numBytesRequested: 32);
            var sb = new StringBuilder();
            foreach (var item in hashed) sb.Append(item.ToString("x2"));
            return sb.ToString();
        }

        public static string ToEncrypted(this string clearText, string owner)
        {
            string EncryptionKey = owner.ToUpper() + eKey;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bt, 15000);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string ToDecrypted(this string cipherText, string owner)
        {
            string EncryptionKey = owner.ToUpper().Trim() + eKey;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bt, 15000);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string Transform(this string txt)
        {
            if (txt.Length < 15) throw new Exception("Your key must not be less than 15 characters long.");
            return txt.Insert(8, "/ytt5uTE&t2").Insert(4, "TEyu$RY7488");
        }
    }

}
