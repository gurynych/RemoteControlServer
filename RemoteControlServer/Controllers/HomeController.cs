using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoteControlServer.BusinessLogic;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;

namespace RemoteControlServer.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ConnectedDevicesService connectedDevices;
        private readonly IDbRepository dbRepository;
        private readonly User user;

        public HomeController(ILogger<HomeController> logger, ConnectedDevicesService connectedDevices, IDbRepository dbRepository)
        {
            this.logger = logger;
            this.connectedDevices = connectedDevices;
            this.dbRepository = dbRepository;
        }

        
        public async Task<IActionResult> Index()
        {
            User user = await GetUserAsync();
            ViewBag.UserDevices = connectedDevices.GetUserDevices(user.Id);
            return View();
        }

        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private Task<User> GetUserAsync()
        {            
            int id = int.Parse(User.FindFirstValue(ClaimTypes.Name));
            return dbRepository.Users.FindByIdAsync(id);
        }
    }
}