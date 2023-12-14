using NetworkMessage;
using NetworkMessage.CommandsResults;
using NetworkMessage.Communicator;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Intents;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Communicators
{
    public class ConnectedDevice : TcpCryptoClientCommunicator
    {
        private readonly IDeviceRepository deviceRepository;
        public Device Device { get; private set; }

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

        public override async Task<bool> HandshakeAsync(IProgress<int> progress = null, CancellationToken token = default)
        {
            INetworkMessage message;
            PublicKeyInfoResult publicKeyInfo =
                await ReceiveNetworkObjectAsync<PublicKeyInfoResult>(progress, token).ConfigureAwait(false);
            if (publicKeyInfo == default) throw new NullReferenceException(nameof(publicKeyInfo));

            byte[] publicKey = await ReceiveBytesAsync(progress, token).ConfigureAwait(false);
            if (publicKey == default || publicKey.Length == 0) throw new NullReferenceException(nameof(publicKey));

            SetExternalPublicKey(publicKey);

            BaseIntent guidIntent = new GuidIntent();
            message = new NetworkMessage.NetworkMessage(guidIntent);
            await SendMessageAsync(message, progress, token).ConfigureAwait(false);
            DeviceGuidResult guidResult = await ReceiveNetworkObjectAsync<DeviceGuidResult>(progress, token).ConfigureAwait(false);
            if (guidResult == null) throw new NullReferenceException(nameof(guidResult));
            Device = await deviceRepository.FindByGuidAsync(guidResult.Guid);
            IsConnected = true;
            SuccessfulTransferResult transferResult = new SuccessfulTransferResult(true);
            message = new NetworkMessage.NetworkMessage(transferResult);
            await SendMessageAsync(message, token: token).ConfigureAwait(false);
            return true;
        }
    }
}
