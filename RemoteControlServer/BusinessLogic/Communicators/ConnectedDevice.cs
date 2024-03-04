using NetworkMessage.CommandsResults.ConcreteCommandResults;
using NetworkMessage.Communicator;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Intents;
using NetworkMessage.Intents.ConcreteIntents;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Communicators
{
    public class ConnectedDevice : TcpCryptoClientCommunicator
    {
        private readonly IDeviceRepository deviceRepository;
        
        public Device Device { get; private set; }
        
        //public Queue<>  { get; set; }

        /// <exception cref="NotImplementedException"/>
        /// <exception cref="ArgumentNullException"/>
        public ConnectedDevice(TcpClient client,
            IAsymmetricCryptographer asymmetricCryptographer,
            ISymmetricCryptographer symmetricCryptographer,
            AsymmetricKeyStoreBase keyStore,
            IDeviceRepository deviceRepository)
            : base(client, asymmetricCryptographer, symmetricCryptographer, keyStore)
        {
            this.deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
        }

        public override async Task<bool> HandshakeAsync(IProgress<long> progress = null, CancellationToken token = default)
        {
            byte[] publicKey;
            using (MemoryStream ms = new MemoryStream())
            {
                await ReceiveStreamAsync(ms, progress, token).ConfigureAwait(false);
                publicKey = ms.ToArray();
            }

            if (publicKey == default || publicKey.Length == 0) throw new NullReferenceException(nameof(publicKey));

            SetExternalPublicKey(publicKey);
            BaseIntent guidIntent = new GuidIntent();            
            await SendObjectAsync(guidIntent, progress, token).ConfigureAwait(false);
            DeviceGuidResult guidResult = await ReceiveAsync<DeviceGuidResult>(progress, token).ConfigureAwait(false);
            if (guidResult == null) throw new NullReferenceException(nameof(guidResult));
            Device = await deviceRepository.FindByGuidAsync(guidResult.Guid, token);
            IsConnected = true;
            SuccessfulTransferResult transferResult = new SuccessfulTransferResult(true);
            await SendObjectAsync(transferResult, token: token).ConfigureAwait(false);
            return true;
        }
    }
}
