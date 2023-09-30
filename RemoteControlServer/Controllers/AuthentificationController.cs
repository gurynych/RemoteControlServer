using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RemoteControlServer.Data;
using RemoteControlServer.Data.Interfaces;
using RemoteControlServer.Data.Models;

namespace RemoteControlServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthentificationController : ControllerBase
    {
        private readonly ILogger<AuthentificationController> logger;
        private readonly ApplicationContext context;
        private readonly IHashCreater hashCreater;
        private readonly ICryptographer cryptographer;

        public AuthentificationController(ILogger<AuthentificationController> logger, ApplicationContext context,
            IHashCreater hashCreater, ICryptographer cryptographer)
        {
            this.logger = logger;
            this.context = context;
            this.hashCreater = hashCreater;
            this.cryptographer = cryptographer;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<AuthentificationController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AuthentificationController>/Authorize
        [HttpPost("Authorize")]
        public bool Post([FromForm] string email, [FromForm] string password)
        {
            User user = context.Users.FirstOrDefault(x => x.Email.Equals(email));
            if (user != null)
            {
                string hash = hashCreater.Hash(password, user.Salt);
                if (user.PasswordHash.Equals(hash))
                {
                    return true;
                }
            }

            return false;
        }

        // POST api/<AuthentificationController>/AuthorizeFromDevice
        [HttpPost("AuthorizeFromDevice")]
        public async Task<byte[]> Post([FromForm] string email, [FromForm] string password, [FromForm] string macAddress)
        {
            User user = await context.Users.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (user != null)
            {
                string hash = hashCreater.Hash(password, user.Salt);
                if (user.PasswordHash.Equals(hash))
                {
                    string macAddressHash = hashCreater.Hash(macAddress, user.Salt);

                    Device device = await context.Devices.FirstOrDefaultAsync(x => x.Hwid.Equals(macAddressHash));
                    if (device == null)
                    {
                        device = new Device()
                        {
                            Hwid = macAddressHash,
                            User = user,
                            UserId = user.Id,
                        };

                        await context.Devices.AddAsync(device);
                        await context.SaveChangesAsync();
                    }

                    return cryptographer.GeneratePublicKey(user.PrivateKey);
                }
            }

            return null;
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