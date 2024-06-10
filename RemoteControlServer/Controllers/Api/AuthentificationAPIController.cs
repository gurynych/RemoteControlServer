using Microsoft.AspNetCore.Mvc;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.Hash;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NuGet.Common;
using NuGet.ContentModel;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;    
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace RemoteControlServer.Controllers.Api
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
        public async Task<IActionResult> AuthorizeFromDevice([FromForm] string email, 
            [FromForm] string password, [FromForm] string deviceGuid,
            [FromForm] string deviceName, [FromForm] string deviceType = null, 
            [FromForm] string devicePlatform = null, [FromForm] string devicePlatformVersion = null,
            [FromForm] string deviceManufacturer = null)
        {
            logger.LogInformation("Try authorize {email}", email);

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(deviceGuid)
                || password.Length < MinPasswordLength || string.IsNullOrWhiteSpace(deviceName)
                || !Regex.IsMatch(email, EmailPattern))
            {
                return BadRequest();
            }

            User user = await dbRepository.Users.FindByEmailAsync(email);

            if (user == null) return NotFound("Пользователя с такими данными не существует");  //NotFound(user);

            string passwordHash = hashCreater.Hash(password, user.Salt);

            if (!user.PasswordHash.Equals(passwordHash)) return Unauthorized();

            Device device = await dbRepository.Devices
                    .FindByGuidAsync(deviceGuid);

            if (device == null)
            {
                device = new Device(deviceGuid, deviceName, user, deviceType, devicePlatform, devicePlatformVersion, deviceManufacturer);
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

            await dbRepository.Users.SaveChangesAsync().ConfigureAwait(false);
            await dbRepository.Devices.SaveChangesAsync().ConfigureAwait(false);
            logger.LogInformation("Success authorize {email}", email);            
            return Ok(publicKey);
        }
        
        [HttpPost("RegisterFromDevice")]
        public async Task<IActionResult> RegisterFromDevice([FromForm] string login,
            [FromForm] string email, [FromForm] string password, 
            [FromForm] string deviceGuid, [FromForm] string deviceName, 
            [FromForm] string deviceType = null, [FromForm] string devicePlatform = null, 
            [FromForm] string devicePlatformVersion = null, [FromForm] string deviceManufacturer = null)
        {
            logger.LogInformation("Try registration: {email}", email);

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(deviceGuid)
                || password.Length < MinPasswordLength || login.Length < MinLoginLength
                || !Regex.IsMatch(email, EmailPattern) || !Regex.IsMatch(password, LoginAndPasswordPattern)
                || !Regex.IsMatch(login, LoginAndPasswordPattern))
            {
                logger.LogError("Error registration: {email}", email);
                return BadRequest("Заполнены не все данные");
            }           

            User user = await dbRepository.Users
                .FindByEmailAsync(email);
            if (user != null)
            {
                logger.LogError("Error registration: {email}", email);
                return Conflict("Такой Email уже зарегестрирован");
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
                device = new Device(deviceGuid, deviceName, user, deviceType, devicePlatform, devicePlatformVersion, deviceManufacturer);
                await dbRepository.Devices.AddAsync(device);
            }

            device.User = user;
            await dbRepository.Users.AddDeviceAsync(user.Id, device);
            await dbRepository.Users.AddAsync(user);
            var publicKey = keyStore.GetPublicKey();
            user.AuthToken = publicKey;

            await dbRepository.Users.SaveChangesAsync();
            logger.LogInformation("Success registration: {email}", email);
            return Ok(publicKey);
        }

        [HttpPost("AuthorizeWithToken")]
        public async Task<IActionResult> AuthorizeWithToken([FromForm] string userToken, 
            [FromForm] string deviceGuid, [FromForm] string deviceName, 
            [FromForm] string deviceType = null, [FromForm] string devicePlatform = null, 
            [FromForm] string devicePlatformVersion = null, [FromForm] string deviceManufacturer = null)
        {
            byte[] token;
            logger.LogInformation("Try authorize with token");
			try
			{
				token = Convert.FromBase64String(WebUtility.UrlDecode(userToken));
			}
			catch
			{
				return BadRequest("User token isn't in base64");
			}

			if (!IsCorrectToken(token))
            {
                return BadRequest();
            }

            User user = await dbRepository.Users.FindByTokenAsync(token);
            if (user == null) return NotFound();

            logger.LogInformation("Success authorize {email} with token", user.Email);
			byte[] publicKey = keyStore.GetPublicKey();
			return Ok(publicKey);
        }

        [HttpGet("GetUserByToken")]
        public async Task<IActionResult> GetUserByToken(string userToken)
        {
            byte[] token = Convert.FromBase64String(userToken);
            logger.LogInformation("Try get user with token");
            if (!IsCorrectToken(token))
            {
                return BadRequest("Токен больше недействителен");
            }

            User user = await dbRepository.Users.FindByTokenAsync(token);
            if (user == null) return NotFound();
            return Ok(new { user.Email, user.Login });
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
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}