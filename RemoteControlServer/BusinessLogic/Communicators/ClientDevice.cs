using Microsoft.EntityFrameworkCore;
using NetworkMessage;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResaults;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Communicators
{
    public class ClientDevice : TcpClientCryptoCommunicator
    {
        private readonly ApplicationContext context;
        private readonly IDbRepository dbRepository;

        public Device Device { get; private set; }

        /// <exception cref="NotImplementedException"/>
        /// <exception cref="ArgumentNullException"/>
        public ClientDevice(TcpClient client, IAsymmetricCryptographer cryptographer, 
            AsymmetricKeyStoreBase keyStore, IDbRepository dbRepository)
            //ApplicationContext context)
            : base(client, cryptographer, keyStore)
        {
            //if (context == null) throw new ArgumentNullException(nameof(context));
            //this.context = context;
            this.dbRepository = dbRepository;
        }

        public override void Handshake(CancellationToken token)
        {
            try
            {
                int repeatCount = 0;
                PublicKeyResult publicKeyResult;
                do
                {
                    token.ThrowIfCancellationRequested();
                    if (repeatCount == 3)
                    {
                        throw new SocketException();
                    }

                    publicKeyResult = ReceivePublicKey();
                    repeatCount++;
                } while (publicKeyResult == default);

                SetExternalPublicKey(publicKeyResult.PublicKey);
                HwidCommand hwidCommand = new HwidCommand();
                Send(hwidCommand);

                repeatCount = 0;
                HwidResult hwidResult = Receive() as HwidResult;
                do
                {
                    token.ThrowIfCancellationRequested();
                    if (repeatCount == 3)
                    {
                        throw new SocketException();
                    }

                    hwidResult = Receive() as HwidResult;
                    repeatCount++;
                } while (hwidResult == default);

                token.ThrowIfCancellationRequested();
                Device = dbRepository.Devices
                    .FirstOrDefaultAsync(x => x.HwidHash.Equals(hwidResult.Hwid))
                    .Result;

                if (Device == null) throw new NullReferenceException(nameof(Device));
            }
            catch { throw; }
        }
    }
}
