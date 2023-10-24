using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using RemoteControlServer.Models;
using System.Security.Claims;
using System.Security.Policy;
using RemoteControlServer.BusinessLogic.Database.Models;
using NetworkMessage.Cryptography;

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
                User currentUser = await dbRepository.Users.FirstOrDefaultAsync
                    (x => x.Email.Equals(authorizationViewModel.Email));
                if (currentUser is not null && 
                    (currentUser.PasswordHash.Equals(hashCreater.Hash(authorizationViewModel.Password, currentUser.Salt))))
                {
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, currentUser.Login),
                        new Claim(ClaimTypes.Email, currentUser.Email)
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
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, newUser.Login),
                        new Claim(ClaimTypes.Email, newUser.Email)
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

        [HttpPost]
        public async Task<IActionResult> Post(string returnUrl)
        {
            List<Claim> claims = new()
            {
                new(ClaimTypes.Name, "Bob"),
                new(ClaimTypes.Name, "Tom")
            };
            ClaimsIdentity ci = new ClaimsIdentity(claims, "Cookie");
            ClaimsPrincipal cp = new ClaimsPrincipal(ci);
            await HttpContext.SignInAsync(cp);
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnUrl);
        }
    }
}
