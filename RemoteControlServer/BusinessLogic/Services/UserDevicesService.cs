using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;

namespace RemoteControlServer.BusinessLogic.Services
{
	public class UserDevicesService
	{
		private readonly IDbRepository dbRepository;
		private readonly ConnectedDevicesService connectedDevicesService;

		public UserDevicesService(IDbRepository dbRepository, ConnectedDevicesService connectedDevicesService)
        {
			this.dbRepository = dbRepository;
			this.connectedDevicesService = connectedDevicesService;
		}

		public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(int userId, CancellationToken token = default)
		{
			IEnumerable<Device> allDevices = await dbRepository.Devices.GetAllAsync(token).ConfigureAwait(false);
			List<UserDevice> result = new List<UserDevice>();
			foreach (Device device in allDevices)
			{
				if (device.User.Id != userId) continue;
				ConnectedDevice connDev = connectedDevicesService.GetConnectedDeviceByDeviceId(device.Id);
				result.Add(new UserDevice(device, connDev?.IsConnected ?? false));
			}

			return result;
		}

		public async Task<UserDevice> GetUserDeviceAsync(int deviceId, CancellationToken token = default)
		{
			Device device = await dbRepository.Devices.FindByIdAsync(deviceId, token).ConfigureAwait(false);
			ConnectedDevice conDev = connectedDevicesService.GetConnectedDeviceByDeviceId(deviceId);
			return new UserDevice(device, conDev?.IsConnected ?? false);
		}
	}
}
