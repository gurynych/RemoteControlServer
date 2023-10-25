using Microsoft.EntityFrameworkCore;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Database;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class DeviceDbRepository : IDeviceRepository
    {
        private readonly IServiceScope scope;        
        private readonly ILogger<UserDbRepository> logger;

        public DeviceDbRepository(IServiceProvider serviceProvider, ILogger<UserDbRepository> logger)
        {            
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            scope = serviceProvider?.CreateScope() ?? throw new ArgumentNullException(nameof(serviceProvider));            
        }

        public async Task<bool> AddAsync(Device item)
        { 
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                if (await context.Devices.AnyAsync(x => x.Id == item.Id || x.HwidHash.Equals(item.HwidHash)))
                {
                    return false;
                }

                await context.Devices.AddAsync(item);
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
                Device d = await context.Devices.FindAsync(id);
                if (d != null)
                {
                    context.Devices.Remove(d);
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

        public async Task<Device> FindByHwidHashAsync(string hwidHash)
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Devices.Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.HwidHash.Equals(hwidHash));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public async Task<Device> FindByIdAsync(int id)
        {
            try
            {                
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Devices.FindAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }        

        public async Task<IEnumerable<Device>> GetAllAsync()
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Devices.Include(x => x.User).ToListAsync();
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

        public async Task<bool> UpdateAsync(Device item)
        {
            try
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
