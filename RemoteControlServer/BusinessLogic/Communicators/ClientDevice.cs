using Microsoft.EntityFrameworkCore;
using NetworkMessage;
using NetworkMessage.Commands;
using NetworkMessage.CommandsResaults;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Communicators
{
    public class ClientDevice : TcpClientCryptoCommunicator
    {
        private readonly ApplicationContext context;
        public Device Device { get; set; }

        /// <exception cref="NotImplementedException"/>
        /// <exception cref="ArgumentNullException"/>
        public ClientDevice(TcpClient client, IAsymmetricCryptographer cryptographer, 
            AsymmetricKeyStoreBase keyStore, ApplicationContext context)
            : base(client, cryptographer, keyStore)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            this.context = context;
        }

        public override void Handshake()
        {
            try
            {
                int repeatCount = 0;
                PublicKeyResult publicKeyResult;
                do
                {
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
                    if (repeatCount == 3)
                    {
                        throw new SocketException();
                    }

                    hwidResult = Receive() as HwidResult;
                    repeatCount++;
                } while (hwidResult == default);

                Device = context.Devices
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.HwidHash.Equals(hwidResult.Hwid));

                if (Device == null) throw new NullReferenceException(nameof(Device));
            }
            catch { throw; }
        }
    }
}
