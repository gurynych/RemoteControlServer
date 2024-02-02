using RemoteControlServer.BusinessLogic.Communicators;

namespace RemoteControlServer.ViewModels
{
    public class DeviceViewModel
    {
        public int DeviceId { get; set; }

        public ConnectedDevice ConnectedDevice { get; set; } 

        public string Path { get; set; }

        public DeviceViewModel(int deviceId, ConnectedDevice connectedDevice, string path)
        {
            DeviceId = deviceId;
            ConnectedDevice = connectedDevice;
            Path = path;
        }
    }
}
