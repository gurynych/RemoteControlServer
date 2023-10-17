using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.Mocks
{
    public class UserMockRepository : IGenericRepository<User>
    {
        private List<User> users;
        private readonly IHashCreater hashCreator;
        private readonly IAsymmetricCryptographer cryptographer;

        public UserMockRepository(IHashCreater hashCreator, IAsymmetricCryptographer cryptographer)
        {
            this.hashCreator = hashCreator;
            this.cryptographer = cryptographer;
            users = new List<User>();
            AddAsync(new User("gurynych", "gurynych@gmail.com", "gurynychPassword") { Id = 1 });
            AddAsync(new User("ksenon", "ksenon@gmail.com", "ksenonPassword") { Id = 2 });
        }

        public Task<bool> AddAsync(User item)
        {
            item.Salt = hashCreator.GenerateSalt();
            item.PasswordHash = hashCreator.Hash(item.PasswordHash, item.Salt);
            if (users.Any(x => x.Id == item.Id || x.Email.Equals(item.Email)))
            {
                return Task.FromResult(false);
            }

            users.Add(item);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(int id)
        {
            User u = users.FirstOrDefault(x => x.Id == id);
            if (u != null)
            {
                users.Remove(u);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        public Task<User> FindByIdAsync(int id)
        {
            return Task.FromResult(users.FirstOrDefault(x => x.Id == id));
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult(users.AsEnumerable());
        }

        public Task<bool> SaveChangesAsync() => Task.FromResult(true);

        public Task<bool> UpdateAsync(User item)
        {
            User changedUser = users.FirstOrDefault(x => x.Id == item.Id);
            if (changedUser != null)
            {
                System.Reflection.PropertyInfo[] props = changedUser.GetType().GetProperties();
                foreach (System.Reflection.PropertyInfo prop in props)
                {
                    if (prop.Name.Equals(nameof(User.Id))) continue;
                    prop.SetValue(changedUser, prop.GetValue(item));
                }

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<User> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}
