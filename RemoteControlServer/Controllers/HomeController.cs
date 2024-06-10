using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.BusinessLogic.Services;
using RemoteControlServer.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.StaticFiles;

namespace RemoteControlServer.Controllers
{
    [AllowAnonymous]
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

        [Route("")]
        public IActionResult Index()
        {
            //User user = await GetUserAsync();
            //ViewBag.UserDevices = connectedDevices.GetUserDevices(user.Id);
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
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public VirtualFileResult DownloadDesktopApp()
        {
            string path = "~/apps/g-eye.exe";
            _ = new FileExtensionContentTypeProvider().TryGetContentType(Path.GetFileName(path), out var contentType);
            return File(path, contentType ?? "application/octet-stream", "g-eye.exe");
            //return PhysicalFile(path, "application/octet-stream", "g-eye");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public VirtualFileResult DownloadAndroidApp()
        {
            string path = "~/apps/g-eye.apk";
            _ = new FileExtensionContentTypeProvider().TryGetContentType(Path.GetFileName(path), out var contentType);
            return File(path, contentType ?? "application/octet-stream", "g-eye.apk");
            //return PhysicalFile(path, "application/octet-stream", "g-eye");
        }
    }
}