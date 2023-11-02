using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.KeyStore;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;

namespace RemoteControlServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthentificationAPIController : ControllerBase
    {
        private readonly ILogger<AuthentificationAPIController> logger;
        private readonly IDbRepository dbRepository;
        private readonly IHashCreater hashCreater;
        private readonly IAsymmetricCryptographer cryptographer;
        private readonly AsymmetricKeyStoreBase keyStore;

        public AuthentificationAPIController(ILogger<AuthentificationAPIController> logger, IDbRepository dbRepository,
            IHashCreater hashCreater, IAsymmetricCryptographer cryptographer, AsymmetricKeyStoreBase keyStore)
        {
            this.logger = logger;
            this.dbRepository = dbRepository;
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
            User user = await dbRepository.Users.FindByEmailAsync(email);
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
        public async Task<IActionResult> AuthorizeFromDevice([FromForm] string email, [FromForm] string password, [FromForm] string hwidHash)
        {
            logger.LogInformation("Try authorize: {email}", email);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(hwidHash))
            {
                return BadRequest();
            }

            User user = await dbRepository.Users.FindByEmailAsync(email);

            if (user == null) return NotFound();

            string passwordHash = hashCreater.Hash(password, user.Salt);

            if (!user.PasswordHash.Equals(passwordHash)) return BadRequest();

            Device device = await dbRepository.Devices
                    .FindByHwidHashAsync(hwidHash);

            if (device == null)
            {
                device = new Device(hwidHash, user);
                await dbRepository.Devices.AddAsync(device);
            }

            if (!user.Devices.Any(x => x.HwidHash.Equals(hwidHash)))
            {
                await dbRepository.Users.AddDeviceAsync(user.Id, device);
            }

            await dbRepository.Users.SaveChangesAsync();
            var publicKey = keyStore.GetPublicKey();
            return Ok(publicKey);
        }

        // POST api/<AuthentificationController>/RegisterFromDevice
        [HttpPost("RegisterFromDevice")]
        public async Task<IActionResult> RegisterFromDevice([FromForm] string login,
            [FromForm] string email, [FromForm] string password, [FromForm] string hwidHash)
        {
            logger.LogInformation($"Try register: email: {email}");

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password)
                || string.IsNullOrEmpty(hwidHash))
            {
                return BadRequest();
            }
            User user = await dbRepository.Users
                .FindByEmailAsync(email);
            if (user != null) return NoContent();

            user = new User(login, email, password);
            Device device = await dbRepository.Devices
                    .FindByHwidHashAsync(hwidHash);
            if (device == null)
            {
                device = new Device(hwidHash, user);
                await dbRepository.Devices.AddAsync(device);
            }

            await dbRepository.Users.AddDeviceAsync(user.Id,device);
            await dbRepository.Users.AddAsync(user);
            await dbRepository.Users.SaveChangesAsync();
            var publicKey = keyStore.GetPublicKey();
            return Ok(publicKey);
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