using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class DbRepository : IDbRepository
    {
        public IGenericRepository<User> Users { get; private set; }

        public IGenericRepository<Device> Devices { get; private set; }

        public DbRepository(IGenericRepository<User> userDbRepository, IGenericRepository<Device> deviceDbRepository)
        {
            if (userDbRepository == default) throw new ArgumentNullException(nameof(userDbRepository));
            if (deviceDbRepository == default) throw new ArgumentNullException(nameof(deviceDbRepository));

            Users = userDbRepository;
            Devices = deviceDbRepository;
        }
    }
}
