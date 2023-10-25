using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class DbRepository : IDbRepository
    {
        public IUserRepository Users { get; private set; }

        public IDeviceRepository Devices { get; private set; }

        public DbRepository(IUserRepository userDbRepository, IDeviceRepository deviceDbRepository)
        {
            Users = userDbRepository ?? throw new ArgumentNullException(nameof(userDbRepository));
            Devices = deviceDbRepository ?? throw new ArgumentNullException(nameof(deviceDbRepository));
        }
    }
}
