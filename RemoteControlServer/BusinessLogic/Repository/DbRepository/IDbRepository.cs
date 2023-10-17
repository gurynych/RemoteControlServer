using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public interface IDbRepository
    {
        IGenericRepository<User> Users { get; }

        IGenericRepository<Device> Devices { get; }
    }
}
