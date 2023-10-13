using NetworkMessage.Cryptography;

namespace RemoteControlServer.BusinessLogic.Database.Models
{
    public class Device
    {
        public int Id { get; set; }        

        public string HwidHash { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public Device() 
        { 
        }

        public Device(string hwid, User user)
        {
            User = user;
            UserId = user.Id;
            HwidHash = hwid;
        }
    }
}
