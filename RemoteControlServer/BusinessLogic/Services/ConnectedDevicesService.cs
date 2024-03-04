using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Collections.Concurrent;

namespace RemoteControlServer.BusinessLogic.Services
{
    public class ConnectedDevicesService
    {
        private List<ConnectedDevice> connectedDevices;        

        public ConnectedDevicesService()
        {
            connectedDevices = new List<ConnectedDevice>();			
		}

        public void AddOrReplaceConnectedDevice(ConnectedDevice connectedDevice)
        {
            ArgumentNullException.ThrowIfNull(connectedDevice);
            User deviceUser = connectedDevice.Device.User;
            ConnectedDevice existingCD =
                connectedDevices.FirstOrDefault(x => x.Device.DeviceGuid.Equals(connectedDevice.Device.DeviceGuid)
                    && x.Device.User.Email.Equals(deviceUser.Email, StringComparison.OrdinalIgnoreCase));

            if (existingCD != null)
            {
                connectedDevices.Remove(existingCD);
            }

            connectedDevices.Add(connectedDevice);
        }        

        public ConnectedDevice GetConnectedDeviceByDeviceId(int deviceId)
        {
            return connectedDevices.Find(x => x.Device.Id == deviceId);
        }
    }
}
