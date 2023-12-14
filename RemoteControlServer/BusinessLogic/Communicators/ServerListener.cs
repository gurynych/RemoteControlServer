using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace RemoteControlServer.BusinessLogic.Communicators
{
    public class ServerListener : BackgroundService
    {
        private readonly TcpListener listener;
        private readonly ILogger<ServerListener> logger;
        private readonly IAsymmetricCryptographer asymmetricCryptographer;
        private readonly ISymmetricCryptographer symmetricCryptographer;
        private readonly AsymmetricKeyStoreBase keyStore;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ConnectedDevicesService connectedDevices;

        public ServerListener(ILogger<ServerListener> logger,
            IAsymmetricCryptographer asymmetricCryptographer,
            ISymmetricCryptographer symmetricCryptographer,
            AsymmetricKeyStoreBase keyStore,
            IServiceScopeFactory scopeFactory,
            ConnectedDevicesService connectedDevices)
        {
            this.logger = logger;
            this.asymmetricCryptographer = asymmetricCryptographer;
            this.symmetricCryptographer = symmetricCryptographer;
            this.keyStore = keyStore;
            this.scopeFactory = scopeFactory;
            this.connectedDevices = connectedDevices;
            listener = new TcpListener(IPAddress.Any, 11000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            listener.Start();
            logger.LogInformation("Begin listening connections");
            await using var scope = scopeFactory.CreateAsyncScope();
            IDbRepository dbRepository = scope.ServiceProvider.GetRequiredService<IDbRepository>();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    if (client != null)
                    {
                        logger.LogInformation("Start connection to {client}", client.Client.RemoteEndPoint);
                        ConnectedDevice newCD =
                            new ConnectedDevice(client, asymmetricCryptographer, symmetricCryptographer, keyStore, dbRepository.Devices);
                        CancellationTokenSource tokenSource = new CancellationTokenSource(30000);
                        logger.LogInformation("Handshake with {client}", client.Client.RemoteEndPoint);
                        try
                        {
                            Progress<int> progress = new Progress<int>((int e) => Debug.WriteLine(e));
                            await newCD.HandshakeAsync(progress, tokenSource.Token);

                            User deviceUser = newCD.Device.User;
                            if (deviceUser == null || await dbRepository.Users.FindByEmailAsync(deviceUser.Email) == null)
                            {
                                throw new NullReferenceException(nameof(deviceUser));
                            }
                            
                            connectedDevices.AddOrReplaceConnectedDevices(newCD);
                            logger.LogInformation("Successful connection to {client}", client.Client.RemoteEndPoint);
                        }
                        catch (NullReferenceException nullEx)
                        {
                            logger.LogError("Connected {object} was null. {client}", nullEx.Message, client.Client.RemoteEndPoint);
                            newCD.Dispose();
                        }
                        catch (OperationCanceledException)
                        {
                            logger.LogError("Connection timeout to {client}", client.Client.RemoteEndPoint);
                            newCD.Dispose();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, null, null);
                            newCD.Dispose();
                        }
                        finally
                        {
                            tokenSource.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogCritical("{exMessage}", ex.Message);
                }
            }

            listener.Stop();
        }
    }
}
