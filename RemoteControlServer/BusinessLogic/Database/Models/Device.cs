using NetworkMessage.Cryptography;

namespace RemoteControlServer.BusinessLogic.Database.Models
{
    public class Device
    {
        public int Id { get; set; }        

        public string DeviceGuid { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public Device() 
        { 
        }

        public Device(string deviceGuid, User user)
        {
            User = user;
            UserId = user.Id;
            DeviceGuid = deviceGuid;
        }
    }
}
