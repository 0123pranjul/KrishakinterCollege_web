using System.Security.Cryptography;
using System.Text;

namespace School_CRM.Service
{
    public class Password
    {
        public static string Decrypt(string cipherText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cipherText))
                {
                    throw new ArgumentException("The cipher text cannot be null or empty.");
                }

                string EncryptionKey = "Geolytics_kamil @r#1!3#1";
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(EncryptionKey));

                byte[] iv = new byte[16] { 0x49, 0x76, 0x64, 0x65, 0x4d, 0x4e, 0x65, 0x61, 0x65, 0x43, 0x2d, 0x4e, 0x25, 0x45, 0x67, 0x76 };
                Aes encryptor = Aes.Create();
                encryptor.Mode = CipherMode.CBC;
                byte[] aesKey = new byte[32];
                Array.Copy(key, 0, aesKey, 0, 32);
                encryptor.Key = aesKey;
                encryptor.IV = iv;

                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                using (ICryptoTransform aesDecryptor = encryptor.CreateDecryptor())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Read))
                {
                    byte[] plainBytes = new byte[cipherBytes.Length];
                    int decryptedByteCount = cryptoStream.Read(plainBytes, 0, plainBytes.Length);
                    return Encoding.ASCII.GetString(plainBytes, 0, decryptedByteCount);
                }
            }
            catch (ArgumentException argEx)
            {
                return $"Error: {argEx.Message}";
            }
            catch (FormatException)
            {
                return "Error: The cipher text format is invalid.";
            }
            catch (CryptographicException cryptoEx)
            {
                return $"Error during decryption: {cryptoEx.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }




        public static string Encrypt(string clearText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clearText))
                {
                    throw new ArgumentException("The clear text cannot be null or empty.");
                }

                string EncryptionKey = "Geolytics_kamil @r#1!3#1";
                SHA256 mySHA256 = SHA256Managed.Create();
                byte[] key = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(EncryptionKey));

                byte[] iv = new byte[16] { 0x49, 0x76, 0x64, 0x65, 0x4d, 0x4e, 0x65, 0x61, 0x65, 0x43, 0x2d, 0x4e, 0x25, 0x45, 0x67, 0x76 };
                Aes encryptor = Aes.Create();
                encryptor.Mode = CipherMode.CBC;
                byte[] aesKey = new byte[32];
                Array.Copy(key, 0, aesKey, 0, 32);
                encryptor.Key = aesKey;
                encryptor.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                using (ICryptoTransform aesEncryptor = encryptor.CreateEncryptor())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aesEncryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.ASCII.GetBytes(clearText);
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    byte[] cipherBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
                }
            }
            catch (ArgumentException argEx)
            {
                return $"Error: {argEx.Message}";
            }
            catch (CryptographicException cryptoEx)
            {
                return $"Error during encryption: {cryptoEx.Message}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
