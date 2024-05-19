using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetworkMessage;
using NetworkMessage.CommandsResults;
using NetworkMessage.CommandsResults.ConcreteCommandResults;
using NetworkMessage.Intents;
using NetworkMessage.Intents.ConcreteIntents;
using RemoteControlServer.BusinessLogic;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.BusinessLogic.Services;
using System.Diagnostics;
using System.Security.Claims;
using NetworkMessage.Exceptions;

// ReSharper disable All

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RemoteControlServer.Controllers.Api
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
        private readonly CommandsService commandsService;
        private readonly UserDevicesService devicesService;

        public DeviceAPIController(ILogger<DeviceAPIController> logger, ConnectedDevicesService connectedDevices,
            IDbRepository dbRepository, IWebHostEnvironment webHostEnvironment, CommandsService commandsService,
            UserDevicesService devicesService)
        {
            this.logger = logger;
            this.connectedDevices = connectedDevices;
            this.dbRepository = dbRepository;
            this.webHostEnvironment = webHostEnvironment;
            this.commandsService = commandsService;
            this.devicesService = devicesService;
        }

        /// <summary>
        /// Post метод API для получения вложенных файлов в директорию
        /// Закрытый. Используя для AJAX-запросов
        /// </summary>
        /// <param name="deviceId">Id устройства, у когорого необходимо получить файлы</param>
        /// <param name="path">Путь, по которому необходимо получить файлы</param>
        /// <returns>Список вложенных файлов и директорий</returns>
        [Authorize]
        [HttpPost("GetNestedFilesInfoInDirectoryForServer")]
        public async Task<IActionResult> GetNestedFilesInfoInDirectoryForServer([FromForm] int deviceId,
            [FromForm] string path)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
            try
            {

                logger.LogInformation("GetNestedFilesInfoInDirectory for server {time}", DateTime.Now);
                byte[] userToken = (await GetUserAsync()).AuthToken;
                if (userToken == null) return BadRequest("Empty user token");
                if (deviceId <= 0) return BadRequest("Invalid device id format");
                if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

                Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
                if (device == null) return NotFound("Device not found");
                if (!device.User.AuthToken.SequenceEqual(userToken))
                    return BadRequest("User token is invalid for this device");

                ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
                if (connectedDevices == null) return Forbid("Device isn't connected to server");
                
                return await GetNestedFilesInfoInDirectoryFromDeviceAsync(userToken, deviceId, path, tokenSource.Token);
            }
            finally
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }

        [Authorize]
        [HttpPost("DownloadFileForServer")]
        public async Task DownloadFileForServer([FromForm] int deviceId, [FromForm] string path)
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

        /// <summary>
        /// Post метод API для скачивания выбранного файла
        /// Закрытый. Используя для AJAX-запросов
        /// </summary>
        /// <param name="deviceId">Id устройства, с когорого необходимо скачать файл</param>
        /// <param name="path">Путь, по которому необходимо скачать файл</param>		
        [Authorize]
        [HttpPost("DownloadDirectoryForServer")]
        public async Task DownloadDirectoryForServer([FromForm] int deviceId, [FromForm] string path)
        {
            logger.LogInformation("DownloadFile for server {time}", DateTime.Now);
            byte[] userToken = (await GetUserAsync().ConfigureAwait(false)).AuthToken;
            //if (userToken == null) return BadRequest("Empty user token");
            //if (deviceId <= 0) return BadRequest("Invalid device id format");
            //if (string.IsNullOrWhiteSpace(path)) return BadRequest("Empty path");

            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            //if (device == null) return NotFound("Device not found");
            //if (!device.User.AuthToken.SequenceEqual(userToken)) return BadRequest("User token is invalid for this device");

            ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
            //if (connectedDevices == null) return Forbid("Device isn't connected to server");

            CancellationTokenSource tokenSource = new CancellationTokenSource(500000);
            await DownloadDirectoryFromDeviceAsync(connectedDevice, path, tokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Get метод API для получения вложенных файлов в директорию
        /// Открытый. Используя для клиентов
        /// </summary>
        /// <param name="userToken">Токен, аутентифицирующий сессию пользователя</param>
        /// <param name="deviceId">Id устройства, у когорого необходимо получить файлы</param>
        /// <param name="path">Путь, по которому необходимо получить файлы</param>		
        [HttpGet("GetNestedFilesInfoInDirectory")]
        public async Task<IActionResult> GetNestedFilesInfoInDirectoryForClient(string userToken, int deviceId,
            string path) //[FromForm] int deviceId, [FromForm] string path)
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
            return await GetNestedFilesInfoInDirectoryFromDeviceAsync(token, connectedDevice.Device.Id, path, tokenSource.Token);
        }

        /// <summary>
        /// Get метод API для получения скачивания файала
        /// Открытый. Используя для клиентов
        /// </summary>
        /// <param name="deviceId">Id устройства, с когорого необходимо скачать файл</param>
        /// <param name="path">Путь, по которому необходимо скачть файл</param>		
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
            await DownloadFileFromDeviceAsync(connectedDevice, path, tokenSource.Token).ConfigureAwait(false);
            return Ok();
        }

        /// <summary>
        /// Get метод API для получения подключенных к серверу устройств
        /// Открытый. Используется для клиентов
        /// </summary>
        /// <param name="userToken">Токен, аутентифицирующий сессию пользователя</param>				
        [HttpGet("GetConnectedDevices")]
        public async Task<IActionResult> GetConnectedDevicesForClient(string userToken)
        {
            logger.LogInformation("GetConnectedDevices {time}", DateTime.Now);
            byte[] token = null;
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

            var usersConnectedDevices = (await devicesService.GetUserDevicesAsync(user.Id).ConfigureAwait(false))
                .Where(x => x.IsConnected);
            if (!usersConnectedDevices.Any()) return NotFound("Нет подключенных устройств");

            logger.LogInformation("GetConnectedDevice success {time}", DateTime.Now);
            var result = usersConnectedDevices.Select(x => new
            {
                Id = x.DeviceId,
                x.DeviceName,
                x.DeviceType,
                x.DeviceManufacturer,
                x.DevicePlatform,
                x.DevicePlatformVersion,
                x.IsConnected
            });

            return Ok(result);
        }

        private async Task<IActionResult> GetNestedFilesInfoInDirectoryFromDeviceAsync(byte[] userToken, int deviceId, string path, CancellationToken token = default)
        {
            try
            {
                NestedFilesInfoResult files =
                    await commandsService.SendAsync<NestedFilesInfoResult>
                        (
                            new NestedFilesInfoIntent(path), userToken, deviceId, token: token
                        )
                        .ConfigureAwait(false);

                NestedDirectoriesInfoResult folders =
                    await commandsService.SendAsync<NestedDirectoriesInfoResult>
                        (
                            new NestedDirectoriesInfoIntent(path), userToken, deviceId, token: token
                        )
                        .ConfigureAwait(false);

                return Ok(new
                {
                    files.NestedFilesInfo,
                    folders.NestedDirectoriesInfo
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
            BaseIntent intent;
            string tempPath = Path.Combine(webHostEnvironment.WebRootPath, "temp", Guid.NewGuid().ToString());
            try
            {
                string fileName = path[(path.LastIndexOf('/') + 1)..];
                string extension = path[(path.LastIndexOf('.') + 1)..].ToLower();

                intent = new DownloadFileIntent(path);
                await connectedDevice.SendObjectAsync(intent, token: token).ConfigureAwait(false);

                //IProgress<long> progress = new Progress<long>(i => Debug.WriteLine(i));
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

                HttpContext.Response.Headers.ContentType = contentType;
                HttpContext.Response.Headers.ContentDisposition = $"attachment; filename={fileName}";
                //HttpContext.Response.Headers.ContentLength = length;
                await connectedDevice.ReceiveStreamAsync(HttpContext.Response.BodyWriter.AsStream(), token: token)
                    .ConfigureAwait(false);
                logger.LogInformation("DownloadFile success {time}", DateTime.Now);
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
            BaseIntent intent;
            string tempPath = Path.Combine(webHostEnvironment.WebRootPath, "temp", Guid.NewGuid().ToString());
            try
            {
                string fileName = path[(path.LastIndexOf('/') + 1)..];
                string extension = path[(path.LastIndexOf('.') + 1)..].ToLower();

                intent = new DownloadDirectoryIntent(path);
                await connectedDevice.SendObjectAsync(intent, token: token).ConfigureAwait(false);

                IProgress<long> progress = new Progress<long>(i => Debug.WriteLine(i));
                HttpContext.Response.ContentType = "application/octet-stream";
                HttpContext.Response.Headers.ContentDisposition = $"attachment; filename={fileName}.rar";
                await connectedDevice.ReceiveStreamAsync(HttpContext.Response.BodyWriter.AsStream(), token: token)
                    .ConfigureAwait(false);
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

        /// <summary>
        /// Post метод API для получения технической инфомации об устройсвтве (ЦПУ, ОЗУ и т.д.)
        /// Закрытый. Используя для AJAX-запросов
        /// </summary>
        /// <param name="deviceId">Id устройства, у которого необходимо информацию</param>				
        [HttpPost("GetDeviceStatusesForServer")]
        [Authorize]
        public async Task<IActionResult> GetDeviceStatusesForServer([FromForm] int deviceId)
        {
            logger.LogInformation("GetDeviceStatusesForServer {time}", DateTime.Now);
            User user = await GetUserAsync().ConfigureAwait(false);
            Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
            if (device == null) return NotFound("Device not found");
            if (!device.User.AuthToken.SequenceEqual(user.AuthToken))
                return Forbid("User token is invalid for this device");
            AmountOfRAMResult amountOfRam =
                await commandsService.SendAsync<AmountOfRAMResult>(new AmountOfRAMIntent(), user.AuthToken, deviceId);

            AmountOfOccupiedRAMResult amountOfOccupiedRAM =
                await commandsService.SendAsync<AmountOfOccupiedRAMResult>(new AmountOfOccupiedRAMIntent(),
                    user.AuthToken, deviceId);

            BatteryChargeResult batteryChargeResult =
                await commandsService.SendAsync<BatteryChargeResult>(new BatteryChargePersentageIntent(),
                    user.AuthToken, deviceId);

            PercentageOfCPUUsageResult percentageOfCPUUsageResult =
                await commandsService.SendAsync<PercentageOfCPUUsageResult>(new PercentageOfCPUUsageIntent(),
                    user.AuthToken, deviceId);

            return Ok(new
            {
                amountOfRam.AmountOfRAM,
                amountOfOccupiedRAM.AmountOfOccupiedRAM,
                batteryChargeResult.ButteryChargePercent,
                percentageOfCPUUsageResult.PercentageOfCPUUsage
            });
        }

        [HttpGet("GetDeviceStatuses")]
        public async Task<IActionResult> GetDeviceStatusesForClient(string userToken, int deviceId)
        {
            try
            {
                logger.LogInformation("GetDeviceStatusesForClient {time}", DateTime.Now);
                byte[] token = Convert.FromBase64String(userToken);

                if (token == null) return BadRequest("Empty user token");
                if (deviceId <= 0) return BadRequest("Invalid device id format");

                Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
                if (device == null) return NotFound("Device not found");
                if (!device.User.AuthToken.SequenceEqual(token))
                    return BadRequest("User token is invalid for this device");

                ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
                if (connectedDevices == null) return Forbid("Device isn't connected to server");

                CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
                AmountOfRAMResult amountOfRam =
                    await commandsService.SendAsync<AmountOfRAMResult>(new AmountOfRAMIntent(), token, deviceId, token: tokenSource.Token);

                AmountOfOccupiedRAMResult amountOfOccupiedRAM =
                    await commandsService.SendAsync<AmountOfOccupiedRAMResult>(new AmountOfOccupiedRAMIntent(),
                        token, deviceId, token: tokenSource.Token);

                BatteryChargeResult batteryChargeResult =
                    await commandsService.SendAsync<BatteryChargeResult>(new BatteryChargePersentageIntent(),
                        token, deviceId, token: tokenSource.Token);

                PercentageOfCPUUsageResult percentageOfCPUUsageResult =
                    await commandsService.SendAsync<PercentageOfCPUUsageResult>(new PercentageOfCPUUsageIntent(),
                        token, deviceId, token: tokenSource.Token);

                return Ok(new
                {
                    amountOfRam.AmountOfRAM,
                    amountOfOccupiedRAM.AmountOfOccupiedRAM,
                    batteryChargeResult.ButteryChargePercent,
                    percentageOfCPUUsageResult.PercentageOfCPUUsage
                });
            }
            catch (FormatException)
            {
                return BadRequest("User token isn't in base64");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (DeviceNotConnectedException)
            {
                return Forbid("Device isn't connected to server");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        
        /// <summary>
        /// Post метод API для получения вложенных файлов в директорию
        /// Закрытый. Используя для AJAX-запросов
        /// </summary>
        /// <param name="deviceId">Id устройства, у которого сделать скриншот</param>		
        /// <returns>Список вложенных файлов и директорий</returns>
        [HttpPost("GetScreenshotForServer")]
        [Authorize]
        public async Task GetScreenshotForServer([FromForm] int deviceId)
        {
            try
            {
                User user = await GetUserAsync().ConfigureAwait(false);
                Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
                if (device == null) return; //NotFound("Device not found");
                if (!device.User.AuthToken.SequenceEqual(user.AuthToken)) return; //Forbid("User token is invalid for this device");			
                HttpContext.Response.ContentType = "image/jpeg";
                HttpContext.Response.Headers.ContentDisposition = $"attachment; filename=Screenshot.jpeg";
                await commandsService.ReceiveStream(new ScreenshotIntent(),
                    HttpContext.Response.BodyWriter.AsStream(),
                    user.AuthToken, deviceId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        [HttpGet("GetScreenshot")]
        public async Task<IActionResult> GetScreenshotForClient(string userToken, int deviceId)
        {
            try
            {
                logger.LogInformation("GetScreenshotForClient {time}", DateTime.Now);
                byte[] token = Convert.FromBase64String(userToken);

                if (token == null) return BadRequest("Empty user token");
                if (deviceId <= 0) return BadRequest("Invalid device id format");

                Device device = await dbRepository.Devices.FindByIdAsync(deviceId);
                if (device == null) return NotFound("Device not found");
                if (!device.User.AuthToken.SequenceEqual(token))
                    return BadRequest("User token is invalid for this device");

                ConnectedDevice connectedDevice = connectedDevices.GetConnectedDeviceByDeviceId(deviceId);
                if (connectedDevices == null) return Forbid("Device isn't connected to server");

                CancellationTokenSource tokenSource = new CancellationTokenSource(50000);
                using MemoryStream ms = new MemoryStream(); 
                await commandsService.ReceiveStream(new ScreenshotIntent(), ms, token, deviceId, token: tokenSource.Token);
                return Ok(ms.ToArray());
            }
            catch (FormatException)
            {
                return BadRequest("User token isn't in base64");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout);
            }
            catch (DeviceNotConnectedException)
            {
                return Forbid("Device isn't connected to server");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private Task<User> GetUserAsync()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}