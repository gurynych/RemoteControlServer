using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.Hash;
using System.Text;

namespace RemoteControlServer.Tests
{
    public class RSACryptographerTests
    {
        private IAsymmetricCryptographer CreateCryptographer() => new RSACryptographer();
        private IHashCreater CreateHash() => new BCryptCreater();

        [Fact]
        //RSACryptographer_EncryptedAndDecrypted_ResultStringEqualOriginal
        public void RSACryptographer_EncryptionStringDataInBothDirections_ResultStringEqualOriginal()
        {
            //Arange
            IAsymmetricCryptographer cryptographer = CreateCryptographer();
            string expected = @"Test_1_Тест!%*/\?@#$!";
            byte[] data = Encoding.UTF8.GetBytes(expected);
            byte[] privateKey = cryptographer.GeneratePrivateKey();
            byte[] firstPublicKey = cryptographer.GeneratePublicKey(privateKey);            
            byte[] encryptedData = cryptographer.Encrypt(data, firstPublicKey);            
            byte[] decryptedData = cryptographer.Decrypt(encryptedData, privateKey);            

            //Act
            string actual = Encoding.UTF8.GetString(decryptedData);            

            //Assert
            Assert.Equal(expected, actual);
        }
    }
}