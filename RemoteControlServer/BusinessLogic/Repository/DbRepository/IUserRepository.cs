using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public interface IUserRepository
    {
        Task<bool> AddAsync(User item);
        Task<bool> DeleteAsync(int id);
        Task<User> FindByIdAsync(int id);
        Task<User> FindByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<bool> SaveChangesAsync();
        Task<bool> UpdateAsync(User item);
        Task<bool> AddDeviceAsync(int id, Device device);
    }
}