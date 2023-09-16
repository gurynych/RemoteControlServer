using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using RemoteControlServer.Data.Helpers.Cryptography;
using RemoteControlServer.Data.Interfaces;

namespace RemoteControlServer.Data.Models
{
    public class User
    {
        private string passwordHash;

        public int Id { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public string PasswordHash
        {
            get => passwordHash;
            set => passwordHash = value;
        }

        public string Salt { get; set; }

        public byte[] PrivateKey { get; set; }
        
        public virtual ICollection<Device> Devices { get; set;}

        public User()
        {
        }

        public User(string login, string email, string password, IHashCreater hash, ICryptographer cryptographer)
        {
            Login = login;
            Email = email;
            Salt = hash.GenerateSalt();
            PasswordHash = hash.Hash(password, Salt);
            PrivateKey = cryptographer.GeneratePrivateKey();
        }
    }
}
