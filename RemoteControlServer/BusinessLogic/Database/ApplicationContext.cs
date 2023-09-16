using Microsoft.EntityFrameworkCore;
using RemoteControlServer.Data.Models;

namespace RemoteControlServer.Data
{
    public class ApplicationContext : DbContext
    {
        private readonly string[] args;

        public DbSet<User> Users { get; set; }

        public DbSet<Device> Devices { get; set; }

        public ApplicationContext()
        {                
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }
    }
}
