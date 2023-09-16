namespace RemoteControlServer.Data.Interfaces
{
    public interface INetworkMessage
    {
        int Size { get; }

        byte[] Message { get; }

        //void CreateMessage
    }
}
