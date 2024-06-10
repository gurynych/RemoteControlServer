using Microsoft.AspNetCore.Authentication;
using NetworkMessage.Cryptography.Hash;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Security.Claims;

namespace RemoteControlServer.BusinessLogic.Services
{
	public class AuthenticationService
	{
		private readonly IDbRepository dbRepository;
		private readonly IHashCreater hashCreater;

		public AuthenticationService(IDbRepository dbRepository, IHashCreater hashCreater)
        {
			this.dbRepository = dbRepository;
			this.hashCreater = hashCreater;
		}

        public async Task<User> AuthorizeAsync(string email, string password)
		{
			User user = await dbRepository.Users.FindByEmailAsync(email).ConfigureAwait(false);
			if (user != null &&
				user.PasswordHash.Equals(hashCreater.Hash(password, user.Salt)))
			{
				return user;
			}

			return null;
		}

		public async Task<User> RegisterAsync(string login, string email, string password)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(login, nameof(login));
			ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));
			ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

			User user = new User(login, email, password);
			if (!await dbRepository.Users.AddAsync(user).ConfigureAwait(false))
			{
				return null;
			}

			await dbRepository.Users.SaveChangesAsync().ConfigureAwait(false);
			user = await dbRepository.Users.FindByEmailAsync(user.Email).ConfigureAwait(false);
			return user;
		}

		public async Task RemebmerMeAsync(HttpContext httpContext, List<Claim> claims, string authenticationType)
		{
			ClaimsIdentity identity = new ClaimsIdentity(claims, authenticationType);
			ClaimsPrincipal principal = new ClaimsPrincipal(identity);
			await httpContext.SignInAsync(principal).ConfigureAwait(false);
		}
	}
}
