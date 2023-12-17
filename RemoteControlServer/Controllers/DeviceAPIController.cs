using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using NetworkMessage;
using NetworkMessage.CommandsResults;
using NetworkMessage.Intents;
using RemoteControlServer.BusinessLogic;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Net;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RemoteControlServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceAPIController : ControllerBase
    {
        private readonly ILogger<DeviceAPIController> logger;
        private readonly ConnectedDevicesService connectedDevices;
        private readonly IDbRepository dbRepository;

        public DeviceAPIController(ILogger<DeviceAPIController> logger, ConnectedDevicesService connectedDevices, IDbRepository dbRepository)
        {
            this.logger = logger;
            this.connectedDevices = connectedDevices;
            this.dbRepository = dbRepository;
        }

        [HttpGet("GetInfo/{macAddress}")]
        public string GetInfo(int macAddress)
        {
            //context.Devices.FirstOrDefault(x => x.Mac.Equals(macAddress));
            return "value";
        }

        [Authorize]
        [HttpPost("GetNestedFilesInfoInDirectory")]
        public async Task<IActionResult> GetNestedFilesInfoInDirectory([FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("GetNestedFilesInfoInDirectory for server {time}", DateTime.Now);
            byte[] userToken = (await GetUserAsync()).AuthToken;
            if (userToken == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            return await GetNestedFilesInfoInDirectoryFromDeviceAsync(connectedDevice, path, tokenSource.Token);
        }

        [Authorize]
        [HttpPost("DownloadFile")]
        public async Task<IActionResult> DownloadFile([FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("DownloadFile for server {time}", DateTime.Now);
            byte[] userToken = (await GetUserAsync()).AuthToken;
            if (userToken == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            return await DownloadFileFromDeviceAsync(connectedDevice, path, tokenSource.Token);
        }

        [HttpGet("GetNestedFilesInfoInDirectory")]
        public async Task<IActionResult> GetNestedFilesInfoInDirectoryForCLient(string userToken, int deviceId, string path) //[FromForm] int deviceId, [FromForm] string path)
        {   
            logger.LogInformation("GetNestedFilesInfoInDirectoryForCLient {time}", DateTime.Now);
            byte[] token;
            try
            {
                token = Convert.FromBase64String(userToken);
            }
            catch
            {
                return BadRequest("User token isn't in base64");
            }

            if (token == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");

            if (!device.User.AuthToken.SequenceEqual(token)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");


            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            return await GetNestedFilesInfoInDirectoryFromDeviceAsync(connectedDevice, path, tokenSource.Token);
            }

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFileForClient(string userToken, int deviceId, string path)
        {
            logger.LogInformation("DownloadFileForClient {time}", DateTime.Now);
            byte[] token;
            try
            {
                token = Convert.FromBase64String(userToken);
            }
            catch
            {
                return BadRequest("User token isn't in base64");
            }

            if (token == null) return BadRequest("Empty user token"); 
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(token)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            return await DownloadFileFromDeviceAsync(connectedDevice, path, tokenSource.Token);
        }

        [HttpGet("GetConnectedDevice")]
        public async Task<IActionResult> GetConnectedDeviceForClient(string userToken)
        {
            logger.LogInformation("GetConnectedDevice {time}", DateTime.Now); 
            byte[] token;
            try
            {
                token = Convert.FromBase64String(userToken);
            }
            catch
            {
                return BadRequest("User token isn't in base64");
            }

            if (token == null) return BadRequest("Empty user token");

            User user = await dbRepository.Users.FindByTokenAsync(token);
            if (user == null) return NotFound("Пользователь не найден");

            var usersConnectedDevices = connectedDevices.GetUserDevices(user.Id).Where(x => x.IsConnected);
            if (!usersConnectedDevices.Any()) return NotFound("Нет подключенных устройств");

            logger.LogInformation("GetConnectedDevice success {time}", DateTime.Now);
            var result = usersConnectedDevices.Select(x => new 
            { 
                x.Device.Id, 
                x.Device.DeviceName, 
                x.Device.DeviceType, 
                x.Device.DeviceManufacturer,  
                x.Device.DevicePlatform,
                x.Device.DevicePlatformVersion,
                x.IsConnected
            });

            return Ok(result);
        }

        private async Task<IActionResult> GetNestedFilesInfoInDirectoryFromDeviceAsync(ConnectedDevice connectedDevice, string path, CancellationToken token = default)
        {
            INetworkMessage message;
            BaseIntent intent;
            try
            {
                intent = new NestedFilesInfoIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendMessageAsync(message, token: token);
                NestedFilesInfoResult nestedFiles =
                    await connectedDevice.ReceiveNetworkObjectAsync<NestedFilesInfoResult>(token: token);
                if (nestedFiles.ErrorMessage != null)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, nestedFiles.ErrorMessage);
                }

                intent = new NestedDirectoriesInfoIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendMessageAsync(message, token: token);
                NestedDirectoriesInfoResult nestedDirectories =
                    await connectedDevice.ReceiveNetworkObjectAsync<NestedDirectoriesInfoResult>(token: token);
                if (nestedFiles.ErrorMessage != null)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, nestedDirectories.ErrorMessage);
                }

                logger.LogInformation("GetNestedFilesInfoInDirectory success {time}", DateTime.Now);
                return Ok(new
                {
                    nestedFiles.NestedFilesInfo,
                    nestedDirectories.NestedDirectoriesInfo
                });
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("GetNestedFilesInfoInDirectory canceled {time}", DateTime.Now);
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception ex)
            {
                logger.LogInformation("GetNestedFilesInfoInDirectory {time}\n{ex}", DateTime.Now, ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<IActionResult> DownloadFileFromDeviceAsync(ConnectedDevice connectedDevice, string path, CancellationToken token = default)
        {
            INetworkMessage message;
            BaseIntent intent;
            try
            {
                intent = new DownloadFileIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendMessageAsync(message, token: token);
                DownloadFileResult fileResult =
                    await connectedDevice.ReceiveNetworkObjectAsync<DownloadFileResult>(token: token);
                if (fileResult.ErrorMessage != null)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, fileResult.ErrorMessage);
                }

                string fileName = path[(path.LastIndexOf('/') + 1)..];
                string extension = path[(path.LastIndexOf('.') + 1)..].ToLower();
                string contentType = extension switch
                {
                    "txt" => "text/plain",
                    "png" => "image/png",
                    "jpeg" or "jpg" => "image/jpeg",
                    "mp3" => "audio/mpeg",
                    "mp4" => "video/pm4",
                    "mpeg" => "video/mpeg",
                    "pdf" => "application/pdf",
                    "doc" => "application/msword",
                    "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _ => "application/octet-stream"
                };

                logger.LogInformation("DownloadFile success {time}", DateTime.Now);
                return File(fileResult.File, contentType, fileName);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        private async Task<byte[]> GetAuthTokenFromRequestBodyAsync(CancellationToken token = default)
        {
            HttpContext.Request.EnableBuffering();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    await HttpContext.Request.Body.CopyToAsync(memoryStream, token);
                    return memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    return null;
                }
            }            
        }

        private Task<User> GetUserAsync()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}
