using System.ComponentModel.DataAnnotations.Schema;

namespace RemoteControlServer.BusinessLogic.Database.Models
{
    public class ServerPrivateKey
    {
        private const int KeyLifetimeDays = 14;

        public int Id { get; set; }

        public byte[] PrivateKey { get; set; }

        public DateTime KeyRegistrationDate { get; set;}

        [NotMapped]
        public DateTime KeyExpirationDate => KeyRegistrationDate.AddDays(KeyLifetimeDays);

        public ServerPrivateKey()
        {            
        }

        public ServerPrivateKey(byte[] privateKey, DateTime keyRegistrationDate)
        {
            PrivateKey = privateKey;
            KeyRegistrationDate = keyRegistrationDate;
        }
    }
}
