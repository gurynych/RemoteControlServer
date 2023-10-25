using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.KeyStore;

namespace RemoteControlServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthentificationController : ControllerBase
    {
        private readonly ILogger<AuthentificationController> logger;
        private readonly ApplicationContext context;
        private readonly IHashCreater hashCreater;
        private readonly IAsymmetricCryptographer cryptographer;
        private readonly AsymmetricKeyStoreBase keyStore;

        public AuthentificationController(ILogger<AuthentificationController> logger, ApplicationContext context,
            IHashCreater hashCreater, IAsymmetricCryptographer cryptographer, AsymmetricKeyStoreBase keyStore)
        {
            this.logger = logger;
            this.context = context;
            this.hashCreater = hashCreater;
            this.cryptographer = cryptographer;
            this.keyStore = keyStore;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST api/<AuthentificationController>/Authorize
        [HttpPost("Authorize")]
        public async Task<bool> Post([FromForm] string email, [FromForm] string passwordHash)
        {
            User user = await context.Users.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (user != null)
            {                
                if (user.PasswordHash.Equals(passwordHash))
                {
                    return true;
                }
            }

            return false;
        }

        // POST api/<AuthentificationController>/AuthorizeFromDevice
        [HttpPost("AuthorizeFromDevice")]
        public async Task<byte[]> Post([FromForm] string email, [FromForm] string password, [FromForm] string hwidHash)
        {
            logger.LogInformation("Try authorize: {email}", email);
            var a = context.Users.Include(x => x.Devices);
            User user = await context.Users.Include(x => x.Devices)
                .FirstOrDefaultAsync(x => x.Email.Equals(email));

            if (user != null)
            {
                string passwordHash = hashCreater.Hash(password, user.Salt);
                if (user.PasswordHash.Equals(passwordHash))
                {
                    Device device = await context.Devices.Include(x => x.User)
                        .FirstOrDefaultAsync(x => x.HwidHash.Equals(hwidHash));

                    if (device == null)
                    {
                        device = new Device(hwidHash, user);
                        await context.Devices.AddAsync(device);
                    }

                    if (!user.Devices.Any(x => x.HwidHash.Equals(hwidHash)))
                    {
                        user.Devices.Add(device);
                        await context.SaveChangesAsync();
                    }
                    
                    await context.SaveChangesAsync();
                    var k = keyStore.GetPublicKey();
                    return k;
                }
            }

            return default;
        }

        // PUT api/<AuthentificationController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AuthentificationController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}