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

        public override async Task Handshake(CancellationToken token)
        {
            try
            {
                //r->s->r
                PublicKeyResult publicKeyResult = await ReceivePublicKeyAsync(token);
                SetExternalPublicKey(publicKeyResult.PublicKey);
                HwidCommand hwidCommand = new HwidCommand();
                await SendAsync(hwidCommand, token);
                INetworkObject networkObject = await ReceiveAsync(token);
                if (networkObject is HwidResult hwidResult)
                {
                    token.ThrowIfCancellationRequested();
                    Device = await dbRepository.Devices.FindByHwidHashAsync(hwidResult.Hwid);
                }

                if (Device == null) throw new NullReferenceException(nameof(Device));
            }
            catch { throw; }
        }
    }
}
