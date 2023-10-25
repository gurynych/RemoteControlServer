using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public interface IDeviceRepository
    {
        Task<bool> AddAsync(Device item);
        Task<bool> DeleteAsync(int id);
        Task<Device> FindByIdAsync(int id);
        Task<Device> FindByHwidHashAsync(string hwidHash);        
        Task<IEnumerable<Device>> GetAllAsync();
        Task<bool> SaveChangesAsync();
        Task<bool> UpdateAsync(Device item);
    }
}