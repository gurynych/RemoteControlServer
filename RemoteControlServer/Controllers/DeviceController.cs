using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using RemoteControlServer.BusinessLogic;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.Models;
using System.Security.Claims;

namespace RemoteControlServer.Controllers
{
    [Authorize]
    public class DeviceController : Controller
    {
        private readonly ILogger<DeviceController> logger;
        private readonly ConnectedDevicesService deviceService;
        private readonly IDbRepository dbRepository;

        public DeviceController(ILogger<DeviceController> logger, ConnectedDevicesService deviceService, IDbRepository dbRepository)
        {
            this.logger = logger;
            this.deviceService = deviceService;
            this.dbRepository = dbRepository;
        }

        [HttpGet("[controller]/{id}/{*path}")]
        public async Task<IActionResult> Index(int id, string path)
        {
            User user = await GetUserAsync();
            if (!user.Devices.Any(x => x.Id == id))
            {
                return Forbid();
            }

            ConnectedDevice connected = deviceService.GetConnectedDeviceByDeviceId(id);
            DeviceViewModel model = new DeviceViewModel(id, connected, path);            
            return View(model);
        }

        private Task<User> GetUserAsync()
        {
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}
