using System.Security.Cryptography;
using System.Text;

namespace PointOfSale.Business.Utilities
{
    public class EncryptionHelper
	{
        const string SecretKey = "mySuperSecretKey";

        public static string EncryptString(string plainText)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            byte[] ivBytes;

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.GenerateIV();
                ivBytes = aes.IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(cs))
                        {
                            writer.Write(plainText);
                        }
                    }

                    byte[] encryptedBytes = ms.ToArray();
                    string ivBase64 = Convert.ToBase64String(ivBytes);
                    string encryptedBase64 = Convert.ToBase64String(encryptedBytes);

                    // Concatenar IV y el texto cifrado
                    return ivBase64 + ":" + encryptedBase64;
                }
            }
        }

        public static string DecryptString(string cipherText)
        {
            string[] parts = cipherText.Split(':');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid encrypted text format");

            string ivBase64 = parts[0];
            string encryptedBase64 = parts[1];

            byte[] keyBytes = Encoding.UTF8.GetBytes(SecretKey);
            byte[] ivBytes = Convert.FromBase64String(ivBase64);
            byte[] cipherBytes = Convert.FromBase64String(encryptedBase64);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}