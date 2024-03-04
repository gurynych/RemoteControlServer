using Microsoft.EntityFrameworkCore;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Models;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class DeviceDbRepository : IDeviceRepository
    {
        private readonly ILogger<UserDbRepository> logger;
        private readonly ApplicationContext context;

        public DeviceDbRepository(ILogger<UserDbRepository> logger, ApplicationContext context)
        {            
            this.logger = logger;
            this.context = context;
        }

        public async Task<bool> AddAsync(Device item, CancellationToken token = default)
        { 
            try
            {                 
                if (await context.Devices.AnyAsync(x => x.Id == item.Id || x.DeviceGuid.Equals(item.DeviceGuid)))
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

        public async Task<bool> DeleteAsync(int id, CancellationToken token = default)
        {           
            try
            {
                 
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

        public async Task<Device> FindByGuidAsync(string deviceGuid, CancellationToken token = default)
        {
            try
            {                 
                return await context.Devices.Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.DeviceGuid.Equals(deviceGuid));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }

        public Task<Device> FindByIdAsync(int id, CancellationToken token = default)
        {
            try
            {
                return context.Devices.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }        

        public async Task<IEnumerable<Device>> GetAllAsync(CancellationToken token = default)
        {
            try
            {
                 
                return await context.Devices.Include(x => x.User).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, null, null);
                return default;
            }
        }        

        public async Task<bool> SaveChangesAsync(CancellationToken token = default)
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

        public async Task<bool> UpdateAsync(Device item, CancellationToken token = default)
        {
            try
            {
                 
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
