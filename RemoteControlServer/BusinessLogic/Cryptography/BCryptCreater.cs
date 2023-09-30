using RemoteControlServer.Data.Interfaces;

namespace RemoteControlServer.BusinessLogic.Cryptography
{
    public class BCryptCreater : IHashCreater
    {
        public string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public string Hash(string data, string salt)
        {
            if (!string.IsNullOrEmpty(data) && !string.IsNullOrEmpty(salt))
            {
                return BCrypt.Net.BCrypt.HashPassword(data, salt);
            }

            throw new ArgumentNullException(nameof(data));
        }
    }
}
