using Microsoft.EntityFrameworkCore;
using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using System.Linq.Expressions;

namespace RemoteControlServer.BusinessLogic.Repository.DbRepository
{
    public class UserDbRepository : IGenericRepository<User>
    {
        private readonly IServiceProvider serviceProvider;        
        private readonly ILogger<UserDbRepository> logger;
        private readonly IHashCreater hashCreator;        

        public UserDbRepository(IServiceProvider serviceProvider, ILogger<UserDbRepository> logger, IHashCreater hashCreator)
        {
            if (serviceProvider == default) throw new ArgumentNullException(nameof(UserDbRepository.serviceProvider));
            if (logger == default) throw new ArgumentNullException(nameof(logger));
            if (hashCreator == default) throw new ArgumentNullException(nameof(hashCreator));            

            this.serviceProvider = serviceProvider;            
            this.logger = logger;
            this.hashCreator = hashCreator;
        }

        public async Task<bool> AddAsync(User item)
        {
            try
            {
                item.Salt = hashCreator.GenerateSalt();
                item.PasswordHash = hashCreator.Hash(item.PasswordHash, item.Salt);

                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    if (await context.Users.AnyAsync(x => x.Id == item.Id || x.Email.Equals(item.Email)))
                    {
                        return false;
                    }

                    await context.Users.AddAsync(item);
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
                    User u = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
                    if (u != null)
                    {
                        context.Users.Remove(u);
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

        public async Task<User> FindByIdAsync(int id)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                try
                {
                    return await context.Users.Include(x => x.Devices).FirstOrDefaultAsync(x => x.Id == id);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, null, null);
                    return default;
                }
            }
        }

        public async Task<User> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Users.Include(x => x.Devices).FirstOrDefaultAsync(predicate);
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                return await context.Users.Include(x => x.Devices).ToListAsync();
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

        public async Task<bool> UpdateAsync(User item)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                    User changedUser = await context.Users.FirstOrDefaultAsync(x => x.Id == item.Id);
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
