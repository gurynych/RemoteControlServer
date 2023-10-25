using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.Mocks
{
    public class DeviceMockRepository : IGenericRepository<Device>
    {
        private readonly IHashCreater hash;
        private readonly IAsymmetricCryptographer cryptographer;
        private List<Device> devices;

        public DeviceMockRepository(IHashCreater hashCreater, IAsymmetricCryptographer cryptographer)
        {
            this.hash = hashCreater;
            this.cryptographer = cryptographer;                       

            List<User> users = new UserMockRepository(hashCreater, cryptographer).GetAllAsync().Result.ToList();
            devices = new List<Device>()
            {
                new Device() { Id = 1, HwidHash = "A64462AFAA94750EEB90B2603B7A4067AFD8A791", UserId = 1, User = users[0] },
                new Device() { Id = 2, HwidHash = "A64462AFAA94750EEB90B2603B7A4067AFD8A791", UserId = 2, User = users[1] }
            };
        }

        public Task<bool> AddAsync(Device item)
        {
            if (devices.Any(x => x.Id == item.Id || x.HwidHash == item.HwidHash))
            {                
                return Task.FromResult(false);
            }

            devices.Add(item);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            Device d = devices.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                devices.Remove(d);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<Device> FindByIdAsync(int id)
        {
            return Task.FromResult(devices.FirstOrDefault(x => x.Id == id));
        }        

        public Task<Device> FirstOrDefaultAsync(Expression<Func<Device, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Device>> GetAllAsync()
        {
            return Task.FromResult(devices.AsEnumerable());
        }

        public Task<bool> SaveChangesAsync() => Task.FromResult(true);

        public Task<bool> UpdateAsync(Device item)
        {
            Device changedUser = devices.FirstOrDefault(x => x.Id == item.Id);
            if (changedUser != null)
            {
                System.Reflection.PropertyInfo[] props = changedUser.GetType().GetProperties();
                foreach (System.Reflection.PropertyInfo prop in props)
                {
                    if (prop.Name.Equals(nameof(Device.Id))) continue;
                    prop.SetValue(changedUser, prop.GetValue(item));
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
