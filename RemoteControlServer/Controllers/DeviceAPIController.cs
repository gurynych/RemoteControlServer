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
using System.Security.Claims;
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
            byte[] userToken = (await GetUserAsync()).AuthToken;
            if (userToken == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            return await GetNestedFilesInfoInDirectoryFromDeviceAsync(connectedDevice, path);
        }

        [Authorize]
        [HttpPost("DownloadFile")]
        public async Task<IActionResult> DownloadFile([FromForm] int deviceId, [FromForm] string path)
        {
            byte[] userToken = (await GetUserAsync()).AuthToken;
            if (userToken == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            return await DownloadFileFromDeviceAsync(connectedDevice, path);
        }

        [HttpGet("GetNestedFilesInfoInDirectory")]
        public async Task<IActionResult> GetNestedFilesInfoInDirectoryForCLient([FromForm] byte[] userToken, [FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("GetNestedFilesInfoInDirectoryForCLient");
            if (userToken == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");

            if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            return await GetNestedFilesInfoInDirectoryFromDeviceAsync(connectedDevice, path);
        }

        [HttpGet("DownloadFile")]
        public async Task<IActionResult> DownloadFileForClient([FromForm] byte[] userToken, [FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("DownloadFileForClient");
            if (userToken == null) return BadRequest("Empty user token");
            if (deviceId <= 0) return BadRequest("Invalid device id format");
            if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            if (connectedDevices == null) return Forbid("Device isn't connected to server");

            return await DownloadFileFromDeviceAsync(connectedDevice, path);
        }

        [HttpPost("GetConnectedDevice")]
        public async Task<IActionResult> GetConnectedDevice()
        {
            logger.LogInformation("GetConnectedDevice");
            byte[] userToken = null;
            _ = HttpContext.Request.Body;
            HttpContext.Request.EnableBuffering();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    await HttpContext.Request.Body.CopyToAsync(memoryStream);
                    userToken = memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }

            if (userToken == null) return BadRequest("Empty user token");

            User user = await dbRepository.Users.FindByTokenAsync(userToken);
            if (user == null) return NotFound("Пользователь не найден");

            var usersConnectedDevices = connectedDevices.GetUserDevices(user.Id).Where(x => x.IsConnected);
            if (!usersConnectedDevices.Any()) return NotFound("Нет подключенных устройств");

            var result = usersConnectedDevices.Select(x => new 
            { 
                x.Device.Id, 
                x.Device.DeviceName, 
                x.Device.DeviceType, 
                x.Device.DeviceManufacturer,  
                x.Device.DevicePlatform,
                x.Device.DevicePlatformVersion
            });

            return Ok(result);
        }

        private async Task<IActionResult> GetNestedFilesInfoInDirectoryFromDeviceAsync(ConnectedDevice connectedDevice, string path)
        {
            INetworkMessage message;
            BaseIntent intent;
            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            try
            {
                intent = new NestedFilesInfoIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendMessageAsync(message, token: tokenSource.Token);
                NestedFilesInfoResult nestedFiles =
                    await connectedDevice.ReceiveNetworkObjectAsync<NestedFilesInfoResult>(token: tokenSource.Token);
                if (nestedFiles.ErrorMessage != null)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, nestedFiles.ErrorMessage);
                }

                intent = new NestedDirectoriesInfoIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendMessageAsync(message, token: tokenSource.Token);
                NestedDirectoriesInfoResult nestedDirectories =
                    await connectedDevice.ReceiveNetworkObjectAsync<NestedDirectoriesInfoResult>(token: tokenSource.Token);
                if (nestedFiles.ErrorMessage != null)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, nestedDirectories.ErrorMessage);
                }

                _ = nestedFiles.NestedFilesInfo;
                _ = nestedDirectories.NestedDirectoriesInfo;
                logger.LogInformation("GetNestedFilesInfoInDirectory success");
                return Ok(new
                {
                    nestedFiles.NestedFilesInfo,
                    nestedDirectories.NestedDirectoriesInfo
                });
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("GetNestedFilesInfoInDirectory canceled");
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception ex)
            {
                logger.LogInformation("GetNestedFilesInfoInDirectory " + ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<IActionResult> DownloadFileFromDeviceAsync(ConnectedDevice connectedDevice, string path)
        {
            INetworkMessage message;
            BaseIntent intent;
            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            try
            {
                intent = new DownloadFileIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendMessageAsync(message, token: tokenSource.Token);
                DownloadFileResult fileResult =
                    await connectedDevice.ReceiveNetworkObjectAsync<DownloadFileResult>(token: tokenSource.Token);
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

        private Task<User> GetUserAsync()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}
