using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.Models;
using System.Security.Claims;
using RemoteControlServer.BusinessLogic.Database.Models;
using NetworkMessage.Cryptography.Hash;

namespace RemoteControlServer.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private IDbRepository dbRepository;

        private IHashCreater hashCreater;

        public AccountController(IDbRepository dbRepository,IHashCreater hashCreater)
        {
            this.dbRepository = dbRepository;
            this.hashCreater = hashCreater;
        }
           

        [HttpGet]
        public IActionResult Authorization()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Authorization([FromForm] AuthorizationViewModel authorizationViewModel)
        {
            if (ModelState.IsValid)
            {
                User currentUser = await dbRepository.Users.FindByEmailAsync(authorizationViewModel.Email);
                if (currentUser is not null && 
                    (currentUser.PasswordHash.Equals(hashCreater.Hash(authorizationViewModel.Password, currentUser.Salt))))
                {
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, currentUser.Id.ToString()),
                    };
                    ClaimsIdentity identity = new ClaimsIdentity(claims, "Cookie");
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Home");
                }
                else ModelState.AddModelError(string.Empty, "Такого пользователя не существует," +
                    " проверьте введенные данные");
            }
            return View(authorizationViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Registration([FromForm] RegistrationViewModel registrationViewModel)
        {
            if (ModelState.IsValid)
            {
                User newUser = new User(registrationViewModel.Login, registrationViewModel.Email,
                    registrationViewModel.Password);

                if (await dbRepository.Users.AddAsync(newUser))
                {
                    await dbRepository.Users.SaveChangesAsync();
                    int id = (await dbRepository.Users.FindByEmailAsync(newUser.Email)).Id;
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, id.ToString()),
                    };
                    ClaimsIdentity identity = new ClaimsIdentity(claims, "Cookie");
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Home");
                }
                else ModelState.AddModelError(string.Empty, "Такой пользователь уже существует");
            }
            return View(registrationViewModel);
        }
    }
}
