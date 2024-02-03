using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
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

        public void AddOrReplaceConnectedDevices(ConnectedDevice connectedDevice)
        {
            if (connectedDevice == null) throw new ArgumentNullException(nameof(connectedDevice));

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

        public List<ConnectedDevice> GetUserDevices(int userId)
        {
            return connectedDevices.FindAll(x => x.Device.UserId == userId);
        }

        public ConnectedDevice GetConnectedDeviceByDeviceId(int deviceId)
        {
            return connectedDevices.FirstOrDefault(x => x.Device.Id == deviceId);
        }
    }
}
