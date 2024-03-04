using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Models;

namespace RemoteControlServer.ViewModels
{
    public class DeviceFolderViewModel
    {
        public UserDevice UserDevice { get; set; } 

        public string Path { get; set; }

        public DeviceFolderViewModel(UserDevice userDevice, string path)
        {
            UserDevice = userDevice;
            Path = path;
        }
    }
}
