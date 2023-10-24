using Microsoft.EntityFrameworkCore;
using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        public DbSet<ServerPrivateKey> ServerPrivateKeys { get; set; }

        public ApplicationContext()
        {
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();
            /*var t = Users.Include(x => x.Devices).ToList();
            Devices.Include(x => x.User);
            var c = Users.FirstOrDefault();
            var b = Users.Local.FirstOrDefault().Devices;
            var a = Users.Local.FirstOrDefault().Devices.ToList();*/
        }
    }
}
