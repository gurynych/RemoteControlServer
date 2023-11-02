using Microsoft.EntityFrameworkCore;
using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class UserDbRepository : IUserRepository
    {
        private readonly IServiceScope scope;
        private readonly ILogger<UserDbRepository> logger;
        private readonly IHashCreater hashCreator;

        public UserDbRepository(IServiceProvider serviceProvider, ILogger<UserDbRepository> logger, IHashCreater hashCreator)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.hashCreator = hashCreator ?? throw new ArgumentNullException(nameof(hashCreator));
            scope = serviceProvider?.CreateScope() ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<bool> AddAsync(User item)
        {
            try
            {
                item.Salt = hashCreator.GenerateSalt();
                item.PasswordHash = hashCreator.Hash(item.PasswordHash, item.Salt);
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                if (await context.Users.AnyAsync(x => x.Id == item.Id || x.Email.Equals(item.Email)))
                {
                    return false;
                }

                await context.Users.AddAsync(item);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }

        public async Task<bool> AddDeviceAsync(int id, Device device)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                User user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                if (user.Devices.Any(x => x.HwidHash.Equals(device.HwidHash)))
                {
                    return false;
                }

                user.Devices.Add(device);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                User u = await context.Users.FindAsync(id);
                if (u != null)
                {
                    context.Users.Remove(u);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                User a = await context.Users.Include(x => x.Devices)
                    .FirstOrDefaultAsync(x => x.Email.Equals(email));
                return a;
            }
            catch (NullReferenceException nullEx)
            {
                throw nullEx;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public async Task<User> FindByIdAsync(int id)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Users.Include(x => x.Devices).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Users.Include(x => x.Devices).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                int entries = await context.SaveChangesAsync();
                if (entries < 1)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(User item)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                User changedUser = await context.Users.FirstOrDefaultAsync(x => x.Id == item.Id);
                if (changedUser != null)
                {
                    System.Reflection.PropertyInfo[] props = changedUser.GetType().GetProperties();
                    foreach (System.Reflection.PropertyInfo prop in props)
                    {
                        if (prop.Name.Equals(nameof(User.Id))) continue;
                        prop.SetValue(changedUser, prop.GetValue(item));
                    }

                    await context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }
    }
}
