using AttendenceSystem01.Iservices;
using System.Security.Cryptography;
using System.Text;

namespace AttendenceSystem01.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public EncryptionService(IConfiguration configuration)
        {
            var keyConfig = configuration["Encryption:Key"];
            if (string.IsNullOrEmpty(keyConfig))
                throw new InvalidOperationException("Encryption key not configured.");

            if (IsBase64String(keyConfig))
            {
                _key = Convert.FromBase64String(keyConfig);
            }
            else
            {
                var tmp = Encoding.UTF8.GetBytes(keyConfig);
                if (tmp.Length == 16 || tmp.Length == 24 || tmp.Length == 32)
                    _key = tmp;
                else
                    throw new InvalidOperationException("Encryption key must be 16, 24 or 32 bytes when using raw text. Use a 32-char key for AES-256.");
            }
        }

        public string Encrypt(string plainText)
        {
            if (plainText == null) return null;
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();
            ms.Write(iv, 0, iv.Length);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }
            var cipherBytes = ms.ToArray();
            return Convert.ToBase64String(cipherBytes);
        }

        public string Decrypt(string cipherText)
        {
            if (cipherText == null) return null;
            var cipherBytesWithIv = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[aes.BlockSize / 8];
            Array.Copy(cipherBytesWithIv, 0, iv, 0, iv.Length);

            var ciphertext = new byte[cipherBytesWithIv.Length - iv.Length];
            Array.Copy(cipherBytesWithIv, iv.Length, ciphertext, 0, ciphertext.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, iv);
            using var ms = new MemoryStream(ciphertext);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            var plain = sr.ReadToEnd();
            return plain;
        }

        private static bool IsBase64String(string s)
        {
            Span<byte> buffer = new Span<byte>(new byte[s.Length]);
            return Convert.TryFromBase64String(s, buffer, out _);
        }
    }
}
