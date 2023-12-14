using NetworkMessage.Cryptography;

namespace RemoteControlServer.BusinessLogic.Database.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }        

        public string Salt { get; set; }

        public byte[] AuthToken { get; set; }
        
        public virtual ICollection<Device> Devices { get; set;}

        public User()
        {
        }

        public User(string login, string email, string password)
        {
            Login = login;
            Email = email;
            PasswordHash = password;
        }
    }
}
