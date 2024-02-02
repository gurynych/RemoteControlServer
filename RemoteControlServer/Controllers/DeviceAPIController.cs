using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetworkMessage;
using NetworkMessage.CommandsResults;
using NetworkMessage.Intents;
using RemoteControlServer.BusinessLogic;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Diagnostics;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RemoteControlServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceAPIController : ControllerBase
    {
        private object locker;
        private readonly ILogger<DeviceAPIController> logger;
        private readonly ConnectedDevicesService connectedDevices;
        private readonly IDbRepository dbRepository;
        private readonly IWebHostEnvironment webHostEnvironment;

        public DeviceAPIController(ILogger<DeviceAPIController> logger, ConnectedDevicesService connectedDevices,
            IDbRepository dbRepository, IWebHostEnvironment webHostEnvironment)
        {
            this.logger = logger;
            this.connectedDevices = connectedDevices;
            this.dbRepository = dbRepository;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("GetInfo/{macAddress}")]
        public string GetInfo(int macAddress)
        {
            //context.Devices.FirstOrDefault(x => x.Mac.Equals(macAddress));
            return "value";
        }

        [Authorize]
        [HttpPost("GetNestedFilesInfoInDirectoryForServer")]
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
        [HttpPost("DownloadFileForServer")]
        public async Task DownloadFile([FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("DownloadFile for server {time}", DateTime.Now);
            byte[] userToken = (await GetUserAsync()).AuthToken;
            //if (userToken == null) return BadRequest("Empty user token");
            //if (deviceId <= 0) return BadRequest("Invalid device id format");
            //if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            //if (device == null) return NotFound("Device not found");
            //if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            //if (connectedDevices == null) return Forbid("Device isn't connected to server");

            CancellationTokenSource tokenSource = new CancellationTokenSource(500000);
            await DownloadFileFromDeviceAsync(connectedDevice, path, tokenSource.Token);
        }

        [Authorize]
        [HttpPost("DownloadDirectoryForServer")]
        public async Task DownloadDirectory([FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("DownloadFile for server {time}", DateTime.Now);
            byte[] userToken = (await GetUserAsync()).AuthToken;
            //if (userToken == null) return BadRequest("Empty user token");
            //if (deviceId <= 0) return BadRequest("Invalid device id format");
            //if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            //if (device == null) return NotFound("Device not found");
            //if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            //if (connectedDevices == null) return Forbid("Device isn't connected to server");

            CancellationTokenSource tokenSource = new CancellationTokenSource(500000);
            await DownloadDirectoryFromDeviceAsync(connectedDevice, path, tokenSource.Token);
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
            //await DownloadFileFromDeviceAsync(connectedDevice, path, length, tokenSource.Token).ConfigureAwait(false);
            return Ok();
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
                await connectedDevice.SendObjectAsync(intent, token: token);
                NestedFilesInfoResult nestedFiles =
                    await connectedDevice.ReceiveNetworkObjectAsync<NestedFilesInfoResult>(token: token);
                if (nestedFiles.ErrorMessage != null)
                {
                    return StatusCode(StatusCodes.Status406NotAcceptable, nestedFiles.ErrorMessage);
                }

                intent = new NestedDirectoriesInfoIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendObjectAsync(intent, token: token);
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

        private async Task DownloadFileFromDeviceAsync(ConnectedDevice connectedDevice, string path, CancellationToken token = default)
        {
            INetworkMessage message;
            BaseIntent intent;
            string tempPath = Path.Combine(webHostEnvironment.WebRootPath, "temp", Guid.NewGuid().ToString());
            try
            {
                string fileName = path[(path.LastIndexOf('/') + 1)..];
                string extension = path[(path.LastIndexOf('.') + 1)..].ToLower();

                intent = new DownloadFileIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendObjectAsync(intent, token: token).ConfigureAwait(false);

                IProgress<long> progress = new Progress<long>(i => Debug.WriteLine(i));
                //await connectedDevice.ReceiveFileAsync(tempPath, progress, token).ConfigureAwait(false);
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

                HttpContext.Response.ContentType = contentType;
                HttpContext.Response.Headers.ContentDisposition = $"attachment; filename={fileName}";
                //HttpContext.Response.Headers.ContentLength = length;
                await connectedDevice.ReceiveStreamAsync(HttpContext.Response.Body, token: token).ConfigureAwait(false);

                logger.LogInformation("DownloadFile success {time}", DateTime.Now);
                //return PhysicalFile(tempPath, contentType, fileName);
            }
            catch (OperationCanceledException)
            {
                //return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        private async Task DownloadDirectoryFromDeviceAsync(ConnectedDevice connectedDevice, string path, CancellationToken token = default)
        {
            INetworkMessage message;
            BaseIntent intent;
            string tempPath = Path.Combine(webHostEnvironment.WebRootPath, "temp", Guid.NewGuid().ToString());
            try
            {
                string fileName = path[(path.LastIndexOf('/') + 1)..];
                string extension = path[(path.LastIndexOf('.') + 1)..].ToLower();

                intent = new DownloadDirectoryIntent(path);
                message = new NetworkMessage.NetworkMessage(intent);
                await connectedDevice.SendObjectAsync(intent, token: token).ConfigureAwait(false);

                IProgress<long> progress = new Progress<long>(i => Debug.WriteLine(i));
                HttpContext.Response.ContentType = "application/octet-stream";
                HttpContext.Response.Headers.ContentDisposition = $"attachment; filename={fileName}";
                await connectedDevice.ReceiveStreamAsync(HttpContext.Response.Body, token: token).ConfigureAwait(false);
                logger.LogInformation("DownloadDirectory success {time}", DateTime.Now);
            }
            catch (OperationCanceledException)
            {
                //return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (Exception ex)
            {
                //return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        private Task<User> GetUserAsync()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}
