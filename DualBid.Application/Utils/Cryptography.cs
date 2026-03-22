using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Libreria.Application.Utils
{
    internal class Cryptography
    {
        public static string Encrypt(string texto, string secret)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(texto);
            string hash = ComputeHash(secret.Substring(0, 32));
            byte[] key = Encoding.UTF8.GetBytes(hash); // 32 bytes        
            byte[] iv = [33, 24, 31, 46, 75, 64, 97, 18, 89, 10, 111, 132, 131, 144, 145, 250]; //16 bytes
            byte[] encryptedBytes;

            // Set up the encryption objects
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt the input plaintext using the AES algorithm
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                }
            }
            //return string encrypt
            return Convert.ToBase64String(encryptedBytes);
        }

        private static string ComputeHash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder();
                foreach (var c in data)
                {
                    sb.Append(c.ToString("x2"));
                }
                return sb.ToString();
            }
        }

    }
}