using RemoteControlServer.Data.Interfaces;
using System.Security.Cryptography;

namespace RemoteControlServer.Data.Helpers.Cryptography
{
    public class RSACryptographer : ICryptographer
    {
        private const int KEY_SIZE = 2048;       

        public byte[] Decrypt(byte[] encryptedData, byte[] privateKey)
        {
            using var rsa = new RSACryptoServiceProvider(KEY_SIZE);
            rsa.ImportCspBlob(privateKey);
            return rsa.Decrypt(encryptedData, true);
        }

        public byte[] Encrypt(byte[] data, byte[] publicKey)
        {
            using var rsa = new RSACryptoServiceProvider(KEY_SIZE);
            rsa.ImportCspBlob(publicKey);
            return rsa.Encrypt(data, true);
        }

        public byte[] GeneratePrivateKey()
        {
            using var rsa = new RSACryptoServiceProvider(KEY_SIZE);
            return rsa.ExportCspBlob(true);
        }

        public byte[] GeneratePublicKey(byte[] privateKey)
        {
            using var rsa = new RSACryptoServiceProvider(KEY_SIZE);
            rsa.ImportCspBlob(privateKey);           
            return rsa.ExportCspBlob(false);
        }        
    }
}
