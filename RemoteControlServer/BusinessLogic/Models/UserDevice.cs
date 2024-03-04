using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Models
{
    public class UserDevice
    {
        public bool IsConnected { get; set; }

        public int DeviceId { get; set; }
                
        public string DeviceGuid { get; set; }

        public string DeviceName { get; set; }

        public string DeviceType { get; set; }

        public string DevicePlatform { get; set; }

        public string DevicePlatformVersion { get; set; }

        public string DeviceManufacturer { get; set; }        

        public UserDevice(Device device, bool isConnected)
        {
            IsConnected = isConnected;
            DeviceId = device.Id;
            DeviceGuid = device.DeviceGuid;
            DeviceName = device.DeviceName;
            DeviceType = device.DeviceType;
            DeviceManufacturer = device.DeviceManufacturer;
            DevicePlatform = device.DevicePlatform;
            DevicePlatformVersion = device.DevicePlatformVersion;
        }
    }
}
