namespace RemoteControlServer.Data.Interfaces
{
    public interface IHashCreater
    {
        string GenerateSalt();

        string Hash(string data, string salt);
    }
}
