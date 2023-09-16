using RemoteControlServer.Data.Interfaces;
using RemoteControlServer.Data.Models;

namespace RemoteControlServer.Data.Repository.Mocks
{
    public class UserMockRepository : IGenericRepository<User>
    {
        private readonly IHashCreater hash;
        private readonly ICryptographer cryptographer;
        private List<User> users;

        public UserMockRepository(IHashCreater hash, ICryptographer cryptographer)
        {
            this.hash = hash;
            this.cryptographer = cryptographer;

            users = new List<User>
            {
                new User("gurynych", "gurynych@gmail.com", "gurynychPassword", hash, cryptographer) {Id = 1},
                new User("hawk0ff", "hawk0ff@gmail.com", "hawk0ffPassword", hash, cryptographer) {Id = 2}  
            };
        }

        public bool AddItem(User item)
        {
            if (users.Any(x => x.Id == item.Id))
            {
                return false;
            }

            users.Add(item);
            return true;
        }

        public bool Delete(int id)
        {
            User u = users.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                users.Remove(u);
                return true;
            }
            
            return false;
        }

        public User Get(int id)
        {
            return users.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<User> GetAll()
        {
            return users;
        }

        public bool SaveChanges() => true;

        public bool Update(User item)
        {
            User changedUser = users.FirstOrDefault(x => x.Id == item.Id);
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
