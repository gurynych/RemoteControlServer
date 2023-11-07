using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Communicators
{
    public class ServerListener
    {
        private readonly TcpListener listener;
        private readonly ILogger<ServerListener> logger;
        private readonly IAsymmetricCryptographer cryptographer;
        private readonly AsymmetricKeyStoreBase keyStore;
        private readonly IDbRepository dbRepository;
        private readonly CancellationTokenSource tokenSource;
        private readonly Thread thread;

        public List<ClientDevice> ClientDevices { get; }

        public ServerListener(ILogger<ServerListener> logger, IAsymmetricCryptographer cryptographer,
            AsymmetricKeyStoreBase keyStore, IDbRepository dbRepository)
        {
            this.logger = logger;
            this.cryptographer = cryptographer;
            this.keyStore = keyStore;
            this.dbRepository = dbRepository;
            listener = new TcpListener(IPAddress.Any, 11000);
            tokenSource = new CancellationTokenSource();
            ClientDevices = new List<ClientDevice>();

            listener.Start();
            thread = new Thread(AcceptSockets) { IsBackground = true };
            thread.Start();
        }

        public void AcceptSockets()
        {

            logger.LogInformation("Begin listening connections");
            while (!tokenSource.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    if (client != null)
                    {
                        logger.LogInformation("Start connection to {client}", client.Client.RemoteEndPoint);
                        ClientDevice newCD = new ClientDevice(client, cryptographer, keyStore, dbRepository);
                        CancellationTokenSource tokenSource = new CancellationTokenSource(15000);
                        _ = Task.Run(async () =>
                        {
                            logger.LogInformation("Handshake with {client}", client.Client.RemoteEndPoint);
                            try
                            {
                                await newCD.Handshake(tokenSource.Token);
                                User deviceUser = newCD.Device.User;
                                if (deviceUser == null || dbRepository.Users.FindByEmailAsync(deviceUser.Email) == null)
                                {
                                    throw new NullReferenceException(nameof(deviceUser));
                                }

                                ClientDevice existingCD = ClientDevices
                                        .FirstOrDefault(x => x.Device.HwidHash.Equals(newCD.Device.HwidHash)
                                                && x.Device.User.Email.Equals(deviceUser.Email, StringComparison.OrdinalIgnoreCase));

                                if (existingCD != null)
                                {
                                    ClientDevices.Remove(existingCD);
                                }

                                ClientDevices.Add(newCD);
                                logger.LogInformation("Connection successful to {client}", client.Client.RemoteEndPoint);
                            }
                            catch (NullReferenceException nullEx)
                            {
                                logger.LogError("Connection {object} was null. {client}", nullEx.Message, client.Client.RemoteEndPoint);
                                client.Close();
                            }
                            catch (OperationCanceledException)
                            {
                                logger.LogError("Connection timeout to {client}", client.Client.RemoteEndPoint);
                                client.Close();
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, null, null);
                                client.Close();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogCritical("{exMessage}", ex.Message);
                }
            }
        }

        public void Stop()
        {
            listener.Stop();
            thread.Join();
            tokenSource.Dispose();
        }
    }
}
