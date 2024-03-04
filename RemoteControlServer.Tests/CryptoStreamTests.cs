using Microsoft.AspNetCore.Components.Forms;
using NetworkMessage.Cryptography.SymmetricCryptography;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace RemoteControlServer.Tests
{
    public class CryptoStreamTests
    {
        private ISymmetricCryptographer CreateSymCryptographer() => new AESCryptographer();

        //private byte[] PrivateKey 


        [Fact]
        public async Task TestSymCryptographerAsync()
        {
            //Arange             
            string expected = @"{{""NestedFilesInfo"":[{""Name"":""ABB.eDesign.Services.dll.config"",""CreationDate"":""2016-09-07T13:54:04Z"",""ChangingDate"":""2016-09-07T13:54:04Z"",""Size"":1477,""FullName"":""C:\\ABB.eDesign.Services.dll.config""},{""Name"":""bootTel.dat"",""CreationDate"":""2021-12-10T16:56:09.2701893Z"",""ChangingDate"":""2021-12-10T16:56:09.2701893Z"",""Size"":112,""FullName"":""C:\\bootTel.dat""},{""Name"":""DumpStack.log.tmp"",""CreationDate"":""2020-12-01T23:29:57.1479843Z"",""ChangingDate"":""2024-01-24T19:02:45.9708917Z"",""Size"":12288,""FullName"":""C:\\DumpStack.log.tmp""},{""Name"":""hiberfil.sys"",""CreationDate"":""2023-02-11T14:19:32.6376902Z"",""ChangingDate"":""2024-02-01T16:23:26.1630075Z"",""Size"":6807429120,""FullName"":""C:\\hiberfil.sys""},{""Name"":""OS"",""CreationDate"":""2019-03-25T18:15:32.6645069Z"",""ChangingDate"":""2019-03-25T18:15:32.6645069Z"",""Size"":0,""FullName"":""C:\\OS""},{""Name"":""pagefile.sys"",""CreationDate"":""2023-12-06T23:54:54.1030771Z"",""ChangingDate"":""2024-01-24T19:02:45.9708917Z"",""Size"":11586056192,""FullName"":""C:\\pagefile.sys""},{""Name"":""swapfile.sys"",""CreationDate"":""2023-02-11T14:16:39.8055594Z"",""ChangingDate"":""2024-01-24T19:02:45.9865182Z"",""Size"":16777216,""FullName"":""C:\\swapfile.sys""}],""ErrorMessage"":null,""Exception"":null}}";
            byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
            ISymmetricCryptographer cryptographer = CreateSymCryptographer();
            MemoryStream dataStream = new MemoryStream(expectedBytes);

            CryptoStream cryptoStream;
            MemoryStream encodedDataStream = new MemoryStream();
            byte[] key = cryptographer.Key;
            byte[] IV = cryptographer.IV;
            cryptoStream = new CryptoStream(dataStream, cryptographer.CreateEncryptor(key, IV), CryptoStreamMode.Read);

            //Act
            await cryptoStream.CopyToAsync(encodedDataStream);
            encodedDataStream.Position = 0;
            string actual;
            cryptographer = CreateSymCryptographer();
            cryptoStream =
                new CryptoStream(encodedDataStream, cryptographer.CreateDecryptor(key, IV), CryptoStreamMode.Read);

            MemoryStream actualStream = new MemoryStream();
            await cryptoStream.CopyToAsync(actualStream);
            byte[] actualBytes = actualStream.ToArray();
            actual = Encoding.UTF8.GetString(actualBytes);


            //Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestSymmetricCryptographer()
        {
            AESCryptographer crypto = new AESCryptographer();
            string expected = @"{{""NestedFilesInfo"":{{""Name"":""ABB.eDesign.Services.dll.config"",""CreationDate"":""2016-09-07T13:54:04Z"",""ChangingDate"":""2016-09-07T13:54:04Z"",""Size"":1477,""FullName"":""C:\\ABB.eDesign.Services.dll.config""},{""Name"":""bootTel.dat"",""CreationDate"":""2021-12-10T16:56:09.2701893Z"",""ChangingDate"":""2021-12-10T16:56:09.2701893Z"",""Size"":112,""FullName"":""C:\\bootTel.dat""},{""Name"":""DumpStack.log.tmp"",""CreationDate"":""2020-12-01T23:29:57.1479843Z"",""ChangingDate"":""2024-01-24T19:02:45.9708917Z"",""Size"":12288,""FullName"":""C:\\DumpStack.log.tmp""},{""Name"":""hiberfil.sys"",""CreationDate"":""2023-02-11T14:19:32.6376902Z"",""ChangingDate"":""2024-02-01T16:23:26.1630075Z"",""Size"":6807429120,""FullName"":""C:\\hiberfil.sys""},{""Name"":""OS"",""CreationDate"":""2019-03-25T18:15:32.6645069Z"",""ChangingDate"":""2019-03-25T18:15:32.6645069Z"",""Size"":0,""FullName"":""C:\\OS""},{""Name"":""pagefile.sys"",""CreationDate"":""2023-12-06T23:54:54.1030771Z"",""ChangingDate"":""2024-01-24T19:02:45.9708917Z"",""Size"":11586056192,""FullName"":""C:\\pagefile.sys""},{""Name"":""swapfile.sys"",""CreationDate"":""2023-02-11T14:16:39.8055594Z"",""ChangingDate"":""2024-01-24T19:02:45.9865182Z"",""Size"":16777216,""FullName"":""C:\\swapfile.sys""}],""ErrorMessage"":null,""Exception"":null}}";
            MemoryStream decStream = new MemoryStream(Encoding.UTF8.GetBytes(expected));
            MemoryStream encStream = new MemoryStream();
            Send(decStream, encStream, crypto);
            
            //encStream.Write([0xAA]);
            encStream.Position = 0;
            MemoryStream actualStream = new MemoryStream();
            int messageSize = (int)decStream.Length;

            int blockSize = 16;
            var paddingSize = blockSize - (decStream.Length % blockSize);
            Read(actualStream, encStream, crypto, (int)decStream.Length + paddingSize);

            string actual = Encoding.UTF8.GetString(actualStream.ToArray());
            Assert.Equal(expected, actual);
        }

        private void Send(Stream streamForRead, Stream streamToWrite, AESCryptographer crypto)
        {
            using (CryptoStream cryptoStream =
                new CryptoStream(streamToWrite,
                    crypto.CreateEncryptor(crypto.Key, crypto.IV),
                    CryptoStreamMode.Write,
                    true))
            {
                long commonSend = 0;
                long sizeToSend;
                byte[] buffer = new byte[1024];
                do
                {
                    sizeToSend = Math.Min(buffer.Length, streamForRead.Length - commonSend);
                    streamForRead.ReadExactly(buffer.AsSpan(0, (int)sizeToSend));
                    cryptoStream.Write(buffer.AsSpan(0, (int)sizeToSend));
                    commonSend += sizeToSend;                    
                } while (sizeToSend > 0 && commonSend < streamForRead.Length);
            }
        }

        private void Read(Stream streamToWrite, Stream streamForRead, AESCryptographer crypto, long messageSize)
        {
            using MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream =
                new CryptoStream(streamToWrite, crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Write, true))
            { 
                long commonRead = 0;
                byte[] buffer = new byte[1024];
                long sizeToRead;
                do
                {
                    sizeToRead = Math.Min(buffer.Length, messageSize - commonRead);
                    streamForRead.ReadExactly(buffer.AsSpan(0, (int)sizeToRead));
                    memoryStream.Write(buffer.AsSpan(0, (int)sizeToRead));
                    cryptoStream.Write(memoryStream.ToArray().AsSpan(0, (int)sizeToRead));
                    memoryStream.Position = 0;
                    commonRead += sizeToRead;                    
                } while (sizeToRead > 0 && commonRead < messageSize);
            }            
        }
    }
}