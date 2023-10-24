using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetworkMessage.Cryptography;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace RemoteControlServer.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ServerListener tcpListener;
        private readonly ApplicationContext context;
        private readonly IHashCreater hashCreater;
        private readonly IAsymmetricCryptographer cryptographer;

        public HomeController(ILogger<HomeController> logger, ServerListener tcpListener, ApplicationContext context,
            IHashCreater hashCreater, IAsymmetricCryptographer cryptographer)
        {
            this.logger = logger;
            this.tcpListener = tcpListener;
            this.context = context;
            this.hashCreater = hashCreater;
            this.cryptographer = cryptographer;
        }

        
        public string Index()
        {

            //context.Users.Add(new User("test", "test", "test", hashCreater, cryptographer));
            //await context.SaveChangesAsync();
            return HttpContext.User.FindFirst(ClaimTypes.Name )+ " " + HttpContext.User.FindFirst(ClaimTypes.Email);
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
    }
}