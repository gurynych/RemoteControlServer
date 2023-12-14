using Microsoft.EntityFrameworkCore;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.Hash;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class UserDbRepository : IUserRepository
    {               
        private readonly ILogger<UserDbRepository> logger;
        private readonly IHashCreater hashCreator;
        private readonly ApplicationContext context;

        public UserDbRepository(ILogger<UserDbRepository> logger, IHashCreater hashCreator, ApplicationContext context)
        {
            this.logger = logger;
            this.hashCreator = hashCreator;
            this.context = context;            
        }

        public async Task<bool> AddAsync(User item)
        {
            try
            {
                item.Salt = hashCreator.GenerateSalt();
                item.PasswordHash = hashCreator.Hash(item.PasswordHash, item.Salt);
                // 

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
                User user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }

                if (user.Devices.Any(x => x.DeviceGuid.Equals(device.DeviceGuid)))
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
                return await context.Users.Include(x => x.Devices)
                    .FirstOrDefaultAsync(x => x.Email.Equals(email));
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
                return await context.Users.Include(x => x.Devices).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public async Task<User> FindByTokenAsync(byte[] token)
        {
            try
            {
                return await context.Users.Include(x => x.Devices).FirstOrDefaultAsync(x => x.AuthToken == token);
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
                 
                await context.SaveChangesAsync();
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
