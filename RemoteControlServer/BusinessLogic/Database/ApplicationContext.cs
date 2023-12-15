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
        }
    }
}
