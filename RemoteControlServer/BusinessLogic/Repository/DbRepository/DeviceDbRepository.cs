using Microsoft.EntityFrameworkCore;
using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Database;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class DeviceDbRepository : IGenericRepository<Device>
    {
        private readonly IServiceProvider serviceProvider;        
        private readonly ILogger<UserDbRepository> logger;        

        public DeviceDbRepository(IServiceProvider serviceProvider, ILogger<UserDbRepository> logger)
        {
            if (serviceProvider == default) throw new ArgumentNullException(nameof(serviceProvider));
            if (logger == default) throw new ArgumentNullException(nameof(logger));

            this.serviceProvider = serviceProvider;            
            this.logger = logger;
        }

        public async Task<bool> AddAsync(Device item)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    if (await context.Devices.AnyAsync(x => x.Id == item.Id || x.HwidHash.Equals(item.HwidHash)))
                    {
                        return false;
                    }

                    await context.Devices.AddAsync(item);
                    return true;
                }
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
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    Device d = await context.Devices.FirstOrDefaultAsync(x => x.Id == id);
                    if (d != null)
                    {
                        context.Devices.Remove(d);
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }

        public async Task<Device> FindByIdAsync(int id)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    return await context.Devices.FirstOrDefaultAsync(x => x.Id == id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public async Task<Device> FirstOrDefaultAsync(Expression<Func<Device, bool>> predicate)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Devices.Include(x => x.User).FirstOrDefaultAsync(predicate);
            }
        }

        public async Task<IEnumerable<Device>> GetAllAsync()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Devices.Include(x => x.User).ToListAsync();
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    int entries = await context.SaveChangesAsync();
                    if (entries < 1)
                    {
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Device item)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    Device changedUser = await context.Devices.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (changedUser != null)
                    {
                        System.Reflection.PropertyInfo[] props = changedUser.GetType().GetProperties();
                        foreach (System.Reflection.PropertyInfo prop in props)
                        {
                            if (prop.Name.Equals(nameof(User.Id))) continue;
                            prop.SetValue(changedUser, prop.GetValue(item));
                        }

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return false;
            }
        }
    }
}
