using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
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
        private readonly IServiceProvider serviceProvider;
        private readonly IDbRepository dbRepository;
        private Thread thread;
        private CancellationTokenSource cancelTokenSrc;

        public List<ClientDevice> ClientDevices { get; }

        public ServerListener(ILogger<ServerListener> logger, IAsymmetricCryptographer cryptographer,
            AsymmetricKeyStoreBase keyStore, IDbRepository dbRepository)
        //IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.cryptographer = cryptographer;
            this.keyStore = keyStore;
            //this.serviceProvider = serviceProvider;
            this.dbRepository = dbRepository;
            listener = new TcpListener(IPAddress.Any, 11000);
            cancelTokenSrc = new CancellationTokenSource();
            ClientDevices = new List<ClientDevice>();

            listener.Start();
            thread = new Thread(AcceptSockets) { IsBackground = true };
            thread.Start();
        }

        public void AcceptSockets()
        {
            try
            {
                logger.LogInformation("Begin listening connections");
                while (!cancelTokenSrc.IsCancellationRequested)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    if (client != null)
                    {
                        logger.LogInformation("Start connection to {client}", client.Client.RemoteEndPoint);
                        ClientDevice clientDevice = new ClientDevice(client, cryptographer, keyStore, dbRepository);
                        CancellationTokenSource tokenSource = new CancellationTokenSource(10000);
                        _ = Task.Run(() =>
                        {
                            logger.LogInformation("Handshake with {client}", client.Client.RemoteEndPoint);
                            try
                            {
                                clientDevice.Handshake(tokenSource.Token);
                                if (ClientDevices.Any(x => x.Device.HwidHash.Equals(clientDevice.Device.HwidHash)))
                                {
                                    ClientDevices.Remove(clientDevice);
                                }

                                ClientDevices.Add(clientDevice);
                                logger.LogInformation("Connection successful to {client}", client.Client.RemoteEndPoint);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, null, null);
                                client.Close();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.ToString());
                //thread.Start();
            }
        }

        /*public async Task<INetworkCommandResult> ReceiveAsync()
        {
            //
            NetworkStream? stream = ConnectedDevices.FirstOrDefault()?.TcpClient.GetStream();
            //
            if (stream == null) return null;

            try
            {
                int resultSize = await ReceiveResultSizeAsync(stream);
                if (resultSize == 0) return null;

                string jsonStr = await ReceiveJsonAsync(stream, resultSize);
                if (jsonStr == string.Empty) return null;

                return JsonConvert.DeserializeObject<INetworkCommandResult>(json);
            }
            catch { throw; }
        }*/

        /*private async Task<int> ReceiveResultSizeAsync(NetworkStream stream)
        {
            if (stream != null)
            {
                try
                {
                    byte[] sizeBuffer = new byte[sizeof(int)];
                    int readedBytes = await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);
                    if (readedBytes == sizeBuffer.Length)
                    {
                        return BitConverter.ToInt32(sizeBuffer);
                    }
                }
                catch (ArgumentOutOfRangeException argExc)
                {
                    throw argExc;
                }
                catch { throw; }

            }

            return 0;
        }

        private async Task<string> ReceiveJsonAsync(NetworkStream stream, int resultSize)
        {
            if (stream != null && resultSize != 0)
            {
                try
                {
                    byte[] msgBuffer = new byte[resultSize];
                    resultSize = await stream.ReadAsync(msgBuffer, cancellationTokenSource.Token);
                    if (resultSize == msgBuffer.Length)
                    {
                        return Encoding.UTF8.GetString(msgBuffer);
                    }
                }
                catch { throw; }
            }

            return string.Empty;
        }        

        */

        /*public async Task SendAsync(INetworkObject netObject)
        {
            //
            NetworkStream? stream = ConnectedDevices.FirstOrDefault()?.TcpClient.GetStream();
            //
            if (stream != null)
            {
                byte[] data = netObject.ToByteArray();
                byte[] size = BitConverter.GetBytes(data.Length);
                byte[] sizeData = new byte[size.Length + data.Length];

                Buffer.BlockCopy(size, 0, sizeData, 0, size.Length);
                Buffer.BlockCopy(data, 0, sizeData, size.Length, data.Length);

                await stream.WriteAsync(sizeData, cancellationTokenSource.Token);
                
                //logger.Log(LogLevel.Information, $"{networkMessage.Size} bytes message sent to {stream.Socket.RemoteEndPoint}");
            }
        }*/

        /*public async Task<INetworkObject> ReceiveAsync()
        {
            //
            NetworkStream? stream = ConnectedDevices.FirstOrDefault()?.TcpClient.GetStream();
            //
            if (stream == null) return null;

            try
            {
                byte[] sizeBuffer = new byte[sizeof(int)];
                int readedByte = await stream.ReadAsync(sizeBuffer, 0, sizeof(int), cancellationTokenSource.Token);
                if (sizeof(int) != readedByte) return null;

                int size = BitConverter.ToInt32(sizeBuffer);
                if (size == 0) return null;

                byte[] data = new byte[size];
                await stream.ReadAsync(data, 0, size, cancellationTokenSource.Token);

                string json = Encoding.UTF8.GetString(data);
                if (json == string.Empty) return null;                

                return JsonConvert.DeserializeObject<INetworkCommandResult>(json);
            }
            catch { throw; }
        }*/

        /*public async Task SendAsync(INetworkCommand command)
        {
            //
            NetworkStream stream = ConnectedDevices.FirstOrDefault()?.TcpClient.GetStream();
            //
            if (stream != null)
            {
                byte[] data = command.ToByteArray();
                byte[] size = BitConverter.GetBytes(data.Length);
                byte[] sizeData = new byte[size.Length + data.Length];

                Buffer.BlockCopy(size, 0, sizeData, 0, size.Length);
                Buffer.BlockCopy(data, 0, sizeData, size.Length, data.Length);

                await stream.WriteAsync(sizeData, cancellationTokenSource.Token);

                //logger.Log(LogLevel.Information, $"{networkMessage.Size} bytes message sent to {stream.Socket.RemoteEndPoint}");
            }
        }

        public async Task<INetworkCommandResult> ReceiveAsync()
        {
            //
            NetworkStream stream = ConnectedDevices.FirstOrDefault()?.TcpClient.GetStream();
            //
            if (stream == null) return default;

            try
            {
                byte[] sizeBuffer = new byte[sizeof(int)];
                int readedByte = await stream.ReadAsync(sizeBuffer, 0, sizeof(int), cancellationTokenSource.Token);
                if (sizeof(int) != readedByte) return default;

                int size = BitConverter.ToInt32(sizeBuffer);
                if (size == 0) return default;

                byte[] data = new byte[size];
                await stream.ReadAsync(data, 0, size, cancellationTokenSource.Token);

                string json = Encoding.UTF8.GetString(data);
                if (json == string.Empty) return default;

                return JsonConvert.DeserializeObject<INetworkCommandResult>(json);
            }
            catch { throw; }
        }*/

        public void Stop()
        {
            listener.Stop();
            thread.Join();
            cancelTokenSrc.Dispose();
        }
    }
}
