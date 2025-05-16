using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ESMART.Presentation.Utils
{
    public class SecureFileHelper
    {
        private static readonly string encryptionKey = "ABCd1234!@#$EFGh5678!@#$IJKl890MNOPESMARTHMSAfrica";

        public static void SaveSecureFile(string hotelName, string productKey, DateTime expirationDate)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".cad");
            string filePath = Path.Combine(directoryPath, "key.dat");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                File.SetAttributes(directoryPath, FileAttributes.Hidden);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string dataToSave = $"HotelName:{hotelName};ProductKey:{productKey};ExpirationDate:{expirationDate:yyyy-MM-dd}";

            string encryptedData = EncryptData(dataToSave);
            File.WriteAllText(filePath, encryptedData);
        }

        public static bool TryLoadProductKey(out string hotelName, out string productKey, out DateTime expirationDate)
        {
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".cad");
            string filePath = Path.Combine(directoryPath, "key.dat");

            hotelName = string.Empty;
            productKey = string.Empty;
            expirationDate = DateTime.MinValue;

            if (File.Exists(filePath))
            {
                try
                {
                    string encryptedData = File.ReadAllText(filePath);
                    string decryptedData = DecryptData(encryptedData);

                    var dataParts = decryptedData.Split(';');
                    foreach (var part in dataParts)
                    {
                        var keyValue = part.Split(':');
                        if (keyValue.Length == 2)
                        {
                            switch (keyValue[0])
                            {
                                case "HotelName":
                                    hotelName = keyValue[1];
                                    break;
                                case "ProductKey":
                                    productKey = keyValue[1];
                                    break;
                                case "ExpirationDate":
                                    expirationDate = DateTime.Parse(keyValue[1]);
                                    break;
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading license: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        private static string EncryptData(string plainText)
        {
            byte[] keyBytes = CreateValidKeySize(encryptionKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                    byte[] combinedBytes = new byte[iv.Length + encryptedBytes.Length];
                    Array.Copy(iv, 0, combinedBytes, 0, iv.Length);
                    Array.Copy(encryptedBytes, 0, combinedBytes, iv.Length, encryptedBytes.Length);

                    return Convert.ToBase64String(combinedBytes);
                }
            }
        }

        private static string DecryptData(string encryptedText)
        {
            byte[] combinedBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = CreateValidKeySize(encryptionKey);

            using (Aes aes = Aes.Create())
            {
                aes.Key = keyBytes;

                byte[] iv = new byte[aes.BlockSize / 8];
                byte[] encryptedBytes = new byte[combinedBytes.Length - iv.Length];
                Array.Copy(combinedBytes, 0, iv, 0, iv.Length);
                Array.Copy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        private static byte[] CreateValidKeySize(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length == 16 || keyBytes.Length == 24 || keyBytes.Length == 32)
            {
                return keyBytes;
            }

            byte[] validKey = new byte[32];
            Array.Copy(keyBytes, validKey, Math.Min(keyBytes.Length, validKey.Length));

            return validKey;
        }
    }
}
