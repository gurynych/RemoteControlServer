using Microsoft.AspNetCore.Mvc;
using RemoteControlServer.ViewModels;
using System.Security.Claims;
using RemoteControlServer.BusinessLogic.Database.Models;
using Microsoft.AspNetCore.Authorization;
using RemoteControlServer.BusinessLogic.Services;

namespace RemoteControlServer.Controllers
{
	[AllowAnonymous]
	public class AccountController : Controller
	{
		private const string AuthenticationType = "Cookie";		
		private readonly AuthenticationService authenticationService;

		public AccountController(AuthenticationService authenticationService)
		{
			this.authenticationService = authenticationService;
		}

		[HttpGet]
		public IActionResult Registration()
		{
			return View();
		}

		[HttpGet]
		public IActionResult Authorization(string returnUrl = null)
		{
			return View(new AuthorizationViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Authorization(AuthorizationViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			User currentUser = await authenticationService.AuthorizeAsync(model.Email, model.Password).ConfigureAwait(false);
			if (currentUser == null)
			{
				ModelState.AddModelError(string.Empty, "Неверная почта или пароль");
				return View(model);
			}

			List<Claim> claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, currentUser.Id.ToString())
			};

			await authenticationService.RemebmerMeAsync(HttpContext, claims, AuthenticationType).ConfigureAwait(false);
			if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
			{
				return Redirect(model.ReturnUrl);
			}

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public async Task<IActionResult> Registration([FromForm] RegistrationViewModel registrationViewModel)
		{
			if (ModelState.IsValid)
			{
				return View(registrationViewModel);
			}

			User newUser = await authenticationService
				.RegisterAsync(registrationViewModel.Login, registrationViewModel.Email, registrationViewModel.Password)
				.ConfigureAwait(false);
			if (newUser == null)
			{
				ModelState.AddModelError(string.Empty, "Такой пользователь уже существует");
				return View(registrationViewModel);
			}

			List<Claim> claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, newUser.Id.ToString()),
			};

			await authenticationService.RemebmerMeAsync(HttpContext, claims, AuthenticationType).ConfigureAwait(false);
			return RedirectToAction("Index", "Home");
		}
	}
}
