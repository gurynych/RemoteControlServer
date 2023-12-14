using Microsoft.AspNetCore.Mvc;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.Hash;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NuGet.Common;
using NuGet.ContentModel;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Text;
using System.Text.RegularExpressions;

namespace RemoteControlServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthentificationAPIController : ControllerBase
    {
        private const int MinPasswordLength = 6;
        private const int MinLoginLength = 3;
        private const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        private const string LoginAndPasswordPattern = @"[a-zA-Z0-9]+$";
        private readonly int tokenSize;
        private readonly ILogger<AuthentificationAPIController> logger;
        private readonly IDbRepository dbRepository;
        private readonly IHashCreater hashCreater;
        private readonly IAsymmetricCryptographer asymmetricCryptographer;
        private readonly AsymmetricKeyStoreBase keyStore;        

        public AuthentificationAPIController(ILogger<AuthentificationAPIController> logger, 
            IDbRepository dbRepository,
            IHashCreater hashCreater, 
            IAsymmetricCryptographer asymmetricCryptographer,
            AsymmetricKeyStoreBase keyStore)
        {
            this.logger = logger;
            this.dbRepository = dbRepository;
            this.hashCreater = hashCreater;
            this.asymmetricCryptographer = asymmetricCryptographer;
            this.keyStore = keyStore;            
            tokenSize = keyStore.GetPublicKey().Length;
        }
        
        [HttpPost("AuthorizeFromDevice")]
        public async Task<IActionResult> AuthorizeFromDevice([FromForm] string email, [FromForm] string password, [FromForm] string deviceGuid)
        {
            logger.LogInformation("Try authorize {email}", email);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(deviceGuid)
                || password.Length < MinPasswordLength
                || !Regex.IsMatch(email, EmailPattern) || !Regex.IsMatch(password, LoginAndPasswordPattern))
            {
                return BadRequest();
            }

            User user = await dbRepository.Users.FindByEmailAsync(email);

            if (user == null) return NotFound(user);

            string passwordHash = hashCreater.Hash(password, user.Salt);

            if (!user.PasswordHash.Equals(passwordHash)) return Unauthorized();

            Device device = await dbRepository.Devices
                    .FindByGuidAsync(deviceGuid);

            if (device == null)
            {
                device = new Device(deviceGuid, user);
                await dbRepository.Devices.AddAsync(device);
            }

            if (!user.Devices.Any(x => x.DeviceGuid.Equals(deviceGuid)))
            {
                await dbRepository.Users.AddDeviceAsync(user.Id, device);
            }

            byte[] publicKey = user.AuthToken;
            if (!IsCorrectToken(user.AuthToken))
            {
                publicKey = keyStore.GetPublicKey();
                user.AuthToken = publicKey;
            }            

            await dbRepository.Users.SaveChangesAsync();
            logger.LogInformation("Success authorize {email}", email);
            return Ok(publicKey);
        }
        
        [HttpPost("RegisterFromDevice")]
        public async Task<IActionResult> RegisterFromDevice([FromForm] string login,
            [FromForm] string email, [FromForm] string password, [FromForm] string deviceGuid)
        {
            logger.LogInformation("Try registration: {email}", email);

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(deviceGuid)
                || password.Length < MinPasswordLength || login.Length < MinLoginLength
                || !Regex.IsMatch(email, EmailPattern) || !Regex.IsMatch(password, LoginAndPasswordPattern)
                || !Regex.IsMatch(login, LoginAndPasswordPattern))
            {
                logger.LogError("Error registration: {email}", email);
                return BadRequest();
            }           

            User user = await dbRepository.Users
                .FindByEmailAsync(email);
            if (user != null)
            {
                logger.LogError("Error registration: {email}", email);
                return Conflict("Email already registered");
            }

            user = new User(login, email, password);
            Device device = await dbRepository.Devices
                    .FindByGuidAsync(deviceGuid);
            //TODO: Create MTM Device and Users
            /*if (device != null)
            {
                logger.LogError("Error registration: {email}", email);
                return Conflict("Device already registered");
            }*/ 

            if (device == null)
            {
                device = new Device(deviceGuid, user);
                await dbRepository.Devices.AddAsync(device);
            }

            await dbRepository.Users.AddDeviceAsync(user.Id, device);
            await dbRepository.Users.AddAsync(user);
            var publicKey = keyStore.GetPublicKey();
            user.AuthToken = publicKey;

            await dbRepository.Users.SaveChangesAsync();
            logger.LogInformation("Success registration: {email}", email);
            return Ok(publicKey);
        }

        [HttpGet("AuthorizeWithToken")]
        public async Task<IActionResult> AuthorizeWithToken([FromForm] byte[] token)
        {
            logger.LogInformation("Try authorize with token");
            if (!IsCorrectToken(token))
            {
                return BadRequest();
            }

            User user = await dbRepository.Users.FindByTokenAsync(token);
            if (user == null) return NotFound();

            logger.LogInformation("Success authorize {email} with token", user.Email);
            var requestedUser = new { user.Email, user.Login };
            return Ok(requestedUser);
        }

        private bool IsCorrectToken(byte[] token)
        {
            if (token == default || token.Length != tokenSize) return false;

            try
            {
                string expected = @"Test$!";
                byte[] data = Encoding.UTF8.GetBytes(expected);
                byte[] encryptedData = asymmetricCryptographer.Encrypt(data, token);
                byte[] decryptedData = asymmetricCryptographer.Decrypt(encryptedData, keyStore.PrivateKey);
                string actual = Encoding.UTF8.GetString(decryptedData);
                if (expected != actual)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}