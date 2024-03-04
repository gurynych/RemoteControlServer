using NetworkMessage.CommandsResults;
using NetworkMessage.Communicator;
using NetworkMessage.Exceptions;
using NetworkMessage.Intents;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Exceptions;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;

namespace RemoteControlServer.BusinessLogic.Services
{
    public class CommandsService
    {
        private readonly IDbRepository dbRepository;
        private readonly ConnectedDevicesService connectedDevices;

        public CommandsService(IDbRepository dbRepository, ConnectedDevicesService connectedDevices)
        {
            this.dbRepository = dbRepository;
            this.connectedDevices = connectedDevices;
        }
        
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="AccessDeniedToDeviceException"></exception>        
        private async Task<TcpCryptoClientCommunicator> GetConnectedDeviceAsync(byte[] userToken, int deviceId, CancellationToken token = default)
        {            
            Device device = await dbRepository.Devices.FindByIdAsync(deviceId, token) ?? throw new NullReferenceException("Device not found");
            if (!device.User.AuthToken.SequenceEqual(userToken))
                throw new AccessDeniedToDeviceException();

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            return connectedDevice;
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="AccessDeniedToDeviceException"></exception>        
        /// <exception cref="DeviceNotConnectedException"></exception>       
        /// <exception cref="ArgumentOutOfRangeException"></exception>       
        public async Task ReceiveStream(BaseIntent intent, Stream streamToWrite, byte[] userToken, int deviceId, IProgress<long> progress = null, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(streamToWrite);
            ArgumentNullException.ThrowIfNull(userToken);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId, nameof(deviceId));

            TcpCryptoClientCommunicator communicator = await GetConnectedDeviceAsync(userToken, deviceId, token).ConfigureAwait(false);
            if (communicator == null || !communicator.IsConnected)
                throw new DeviceNotConnectedException();

            await communicator.SendObjectAsync(intent, progress, token).ConfigureAwait(false);
            await communicator.ReceiveStreamAsync(streamToWrite, progress, token).ConfigureAwait(false);
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="AccessDeniedToDeviceException"></exception>        
        /// <exception cref="DeviceNotConnectedException"></exception>       
        /// <exception cref="ArgumentOutOfRangeException"></exception>       
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<TResult> SendAsync<TResult>(BaseIntent intent, byte[] userToken, int deviceId, IProgress<long> progress = null, CancellationToken token = default)
            where TResult : BaseNetworkCommandResult
        {
            ArgumentNullException.ThrowIfNull(intent);
            ArgumentNullException.ThrowIfNull(userToken);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(deviceId, nameof(deviceId));

            TcpCryptoClientCommunicator communicator = await GetConnectedDeviceAsync(userToken, deviceId, token);
            if (communicator == null || !communicator.IsConnected)
                throw new DeviceNotConnectedException();

            await communicator.SendObjectAsync(intent, progress, token);
            return await communicator.ReceiveAsync<TResult>(progress, token);
        }
    }
}
