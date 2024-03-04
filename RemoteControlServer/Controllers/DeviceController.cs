using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.BusinessLogic.Services;
using RemoteControlServer.ViewModels;
using System.IO;
using System.Security.Claims;

namespace RemoteControlServer.Controllers
{
    [Authorize]
    public class DeviceController : Controller
    {
        private readonly ILogger<DeviceController> logger;
		private readonly UserDevicesService devicesService;
		private readonly IDbRepository dbRepository;

        public DeviceController(ILogger<DeviceController> logger, UserDevicesService devicesService, IDbRepository dbRepository)
        {
            this.logger = logger;
			this.devicesService = devicesService;
			this.dbRepository = dbRepository;
        }

        [HttpGet("Devices")]
        public async Task<IActionResult> Devices()
        {
			User user = await GetUserAsync().ConfigureAwait(false);
             return View(new List<UserDevice>(await devicesService.GetUserDevicesAsync(user.Id).ConfigureAwait(false)));
		}

        [HttpGet("[controller]/{id}")]
        public async Task<IActionResult> Device(int id)
        {
            User user = await GetUserAsync().ConfigureAwait(false);
            if (!user.Devices.Any(x => x.Id == id))
            {
                return Forbid();
            }

            UserDevice userDevice = await devicesService.GetUserDeviceAsync(id).ConfigureAwait(false);
            return View(userDevice);
        }

		[HttpGet("DeviceFolder/{id}/{*path}")]
        public async Task<IActionResult> DeviceFolder(int id, string path)
        {
            User user = await GetUserAsync().ConfigureAwait(false);
            if (!user.Devices.Any(x => x.Id == id)) 
            {
                return Forbid();
            }

            UserDevice userDevice = await devicesService.GetUserDeviceAsync(id).ConfigureAwait(false);
            DeviceFolderViewModel model = new DeviceFolderViewModel(userDevice, path);            
            return View(model);           
        }

        private Task<User> GetUserAsync()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}
