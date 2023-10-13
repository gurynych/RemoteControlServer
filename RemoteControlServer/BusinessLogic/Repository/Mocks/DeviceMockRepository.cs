using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Database.Models;

namespace RemoteControlServer.BusinessLogic.Repository.Mocks
{
    public class DeviceMockRepository : IGenericRepository<Device>
    {
        private readonly IHashCreater hash;
        private readonly IAsymmetricCryptographer cryptographer;
        private List<Device> devices;

        public DeviceMockRepository(IHashCreater hash, IAsymmetricCryptographer cryptographer)
        {
            this.hash = hash;
            this.cryptographer = cryptographer;                       

            List<User> users = new UserMockRepository(hash, cryptographer).GetAll().ToList();
            devices = new List<Device>()
            {                                  //gurynychHWID
                new Device() { Id = 1, HwidHash = "7113281bb52c59deb42604b83c5713fe00d81bf4ca180ac1189ff42bac14abea", UserId = 1, User = users[0] },
                                               //hawk0ffHWID
                new Device() { Id = 2, HwidHash = "1fe65d27f3aea8338b04d14c006fc9c382f8f0259233ce97a5d4b2dc4f9ff1ca", UserId = 2, User = users[1] }
            };
        }

        public bool AddItem(Device item)
        {
            if (devices.Any(x => x.HwidHash == item.HwidHash))
            {                
                return false;
            }

            devices.Add(item);
            return true;
        }

        public bool Delete(int id)
        {
            Device d = devices.FirstOrDefault(x => x.Id == id);
            if (d != null)
            {
                devices.Remove(d);
                return true;
            }

            return false;
        }

        public Device Get(int id)
        {
            return devices.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Device> GetAll()
        {
            return devices;
        }

        public bool SaveChanges() => true;

        public bool Update(Device item)
        {
            Device changedUser = devices.FirstOrDefault(x => x.Id == item.Id);
            if (changedUser != null)
            {
                System.Reflection.PropertyInfo[] props = changedUser.GetType().GetProperties();
                foreach (System.Reflection.PropertyInfo prop in props)
                {
                    prop.SetValue(changedUser, prop.GetValue(item));
                }

                return true;
            }

            return false;
        }
    }
}
