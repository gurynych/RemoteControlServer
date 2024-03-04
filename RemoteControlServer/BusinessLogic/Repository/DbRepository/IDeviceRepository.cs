using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public interface IDeviceRepository
    {
        Task<bool> AddAsync(Device item, CancellationToken token = default);
        Task<bool> DeleteAsync(int id, CancellationToken token = default);
        Task<Device> FindByIdAsync(int id, CancellationToken token = default);
        Task<Device> FindByGuidAsync(string hwidHash, CancellationToken token = default);
        Task<IEnumerable<Device>> GetAllAsync(CancellationToken token = default);        
        Task<bool> SaveChangesAsync(CancellationToken token = default);
        Task<bool> UpdateAsync(Device item, CancellationToken token = default);
    }
}