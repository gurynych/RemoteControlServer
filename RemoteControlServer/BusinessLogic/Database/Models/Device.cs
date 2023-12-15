using NetworkMessage.Cryptography;

namespace RemoteControlServer.BusinessLogic.Database.Models
{
    public class Device
    {
        public int Id { get; set; }

        public string DeviceName { get; set; }

        public string DeviceType { get; set; }

        public string DevicePlatform { get; set; }

        public string DevicePlatformVersion { get; set; }

        public string DeviceManufacturer { get; set; }

        public string DeviceGuid { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public Device()
        {
        }

        public Device(string deviceGuid, string deviceName, User user, string deviceType = null, string devicePlatform = null, string devicePlatformVersion = null, string deviceManufacturer = null)
        {
            User = user;
            UserId = user.Id;
            DeviceGuid = deviceGuid;
            DeviceName = deviceName;
            DeviceType = deviceType;
            DevicePlatform = devicePlatform;
            DevicePlatformVersion = devicePlatformVersion;
            DeviceManufacturer = deviceManufacturer;
        }
    }
}
