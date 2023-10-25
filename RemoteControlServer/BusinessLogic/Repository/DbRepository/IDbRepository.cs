using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public interface IDbRepository
    {
        IUserRepository Users { get; }

        IDeviceRepository Devices { get; }
    }
}
