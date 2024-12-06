using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ZstdNet;

namespace ClientMessengerHttpUpdated
{
    internal class Security
    {
        private static readonly Aes _aes;

        static Security()
        {
            _aes = Aes.Create();
            _aes.GenerateIV();
            _aes.GenerateKey();
        }

        internal static async Task SendAes(JsonElement root)
        {
            _ = Logger.LogAsync("Sending aes");
            var publicKey = new RSAParameters()
            {
                Modulus = root.GetProperty("modulus").GetBytesFromBase64(),
                Exponent = root.GetProperty("exponent").GetBytesFromBase64()
            };

            var payload = new
            {
                code = OpCode.SendAes,
                key = Convert.ToBase64String(_aes.Key),
                iv = Convert.ToBase64String(_aes.IV),
            };
            var jsonString = JsonSerializer.Serialize(payload);
            await Client.SendPayloadAsync(jsonString, publicKey);
        }

        #region LoginData
        internal static void SaveLoginData(string email, string password)
        {
            var emailBytes = Encoding.UTF8.GetBytes(email);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var encryptedEmail = ProtectedData.Protect(emailBytes, null, DataProtectionScope.CurrentUser);
            var encryptedPassword = ProtectedData.Protect(passwordBytes, null, DataProtectionScope.CurrentUser);

            var base64EncryptedEmail = Convert.ToBase64String(encryptedEmail);
            var base64EncryptedPassword = Convert.ToBase64String(encryptedPassword);

            using var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
            using var fileStream = new IsolatedStorageFileStream("loginData.txt", FileMode.Create, isolatedStorage);
            using var writer = new StreamWriter(fileStream);
            {
                writer.WriteLine(base64EncryptedEmail);
                writer.WriteLine(base64EncryptedPassword);
            }
        }

        internal static bool TryGetLoginData([NotNullWhen(true)] out string? email, [NotNullWhen(true)] out string? password)
        {
            SaveLoginData("Cristianocx7@gmail.com", "Cristiano");
            using var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();

            if (!isolatedStorage.FileExists("loginData.txt"))
            {
                email = null;
                password = null;
                return false;
            }

            try
            {
                using var fileStream = new IsolatedStorageFileStream("loginData.txt", FileMode.Open, isolatedStorage);
                using var reader = new StreamReader(fileStream);

                var encryptedEmail = Convert.FromBase64String(reader.ReadLine()!);
                var encryptedPassword = Convert.FromBase64String(reader.ReadLine()!);

                var decryptedEmail = ProtectedData.Unprotect(encryptedEmail, null, DataProtectionScope.CurrentUser);
                var decryptedPassword = ProtectedData.Unprotect(encryptedPassword, null, DataProtectionScope.CurrentUser);

                email = Encoding.UTF8.GetString(decryptedEmail);
                password = Encoding.UTF8.GetString(decryptedPassword);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                email = null;
                password = null;
                return false;
            }
        }

        internal static void DeleteLoginData()
        {
            using var isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
            isolatedStorage.DeleteFile("loginData.txt");
        }

        #endregion

        #region Compression/Decompression

        internal static byte[] CompressData(string data)
        {
            var dataAsBytes = Encoding.UTF8.GetBytes(data);
            using var compressor = new Compressor(new CompressionOptions(1));
            var compressedData = compressor.Wrap(dataAsBytes);

            return compressedData.Length >= dataAsBytes.Length
                ? dataAsBytes
                : compressedData;
        }

        internal static byte[] DecompressData(byte[] data)
        {
            try
            {
                using var decompressor = new Decompressor();
                return decompressor.Unwrap(data);
            }
            catch (ZstdException)
            {
                return data;
            }
        }

        #endregion

        #region Encryption

        internal static byte[] EncryptAes(byte[] dataToEncrypt)
        {
            using ICryptoTransform encryptor = _aes.CreateEncryptor();

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                csEncrypt.FlushFinalBlock();
            }

            return msEncrypt.ToArray();
        }

        internal static byte[] EncryptRSA(RSAParameters key, byte[] dataToEncrypt)
        {
            using var rsa = RSA.Create();
            rsa.ImportParameters(key);
            return rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);
        }

        #endregion

        #region Decryption

        internal static JsonElement? DecyrptMessage(string messageToDecyrpt)
        {
            var dataAsBytes = Convert.FromBase64String(messageToDecyrpt);
            try
            {
                var decompressedData = DecompressData(dataAsBytes);
                return JsonDocument.Parse(decompressedData).RootElement;
            }
            catch (Exception)
            {
                return DecryptAes(dataAsBytes);
            }
        }

        private static JsonElement? DecryptAes(byte[] dataToDecrypt)
        {
            try
            {
                using ICryptoTransform decryptor = _aes.CreateDecryptor();
                {
                    using var memoryStream = new MemoryStream(dataToDecrypt);
                    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    using var resultStream = new MemoryStream();
                    {
                        cryptoStream.CopyTo(resultStream);
                    }
                    var decompressedData = DecompressData(resultStream.ToArray());
                    return JsonSerializer.Deserialize<JsonElement>(decompressedData);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return null;
            }
        }
    }

    #endregion
}
