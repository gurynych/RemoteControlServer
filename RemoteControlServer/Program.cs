using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Communicators;
using RemoteControlServer.BusinessLogic.Database;
using RemoteControlServer.BusinessLogic.KeyStore;
using RemoteControlServer.BusinessLogic.Database.Models;
using RemoteControlServer.BusinessLogic.Repository;
using RemoteControlServer.BusinessLogic.Repository.DbRepository;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.Hash;
using NetworkMessage.Cryptography.SymmetricCryptography;
using RemoteControlServer.BusinessLogic;

namespace RemoteControlServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add services to the container.            
            builder.Services.AddControllersWithViews();
            builder.Services.AddMvcCore();            
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<ApplicationContext>(DbContextOptions => DbContextOptions.UseNpgsql(connectionString)
            //Debugging stuff
            //.LogTo(Console.WriteLine, LogLevel.Information)
            //.EnableSensitiveDataLogging()
            //.EnableDetailedErrors()
            );

            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);

            builder.Services.AddMvc(mvcOtions => mvcOtions.EnableEndpointRouting = false);
            builder.Services.AddSingleton<IAsymmetricCryptographer, RSACryptographer>();
            builder.Services.AddSingleton<ISymmetricCryptographer, AESCryptographer>();
            builder.Services.AddSingleton<IHashCreater, BCryptCreater>();
            builder.Services.AddScoped<IUserRepository, UserDbRepository>();
            builder.Services.AddScoped<IDeviceRepository, DeviceDbRepository>();
            builder.Services.AddScoped<IDbRepository, DbRepository>();
            builder.Services.AddSingleton<AsymmetricKeyStoreBase, ServerKeysStore>();
            builder.Services.AddSingleton<ConnectedDevicesService>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => options.LoginPath = "/Account/Authorization");
            builder.Services.AddHostedService<ServerListener>();
            builder.Services.AddAuthorization();

            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();            

            // Configure the HTTP request pipeline.            
            app.UseStaticFiles();
            app.MapControllers();           

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMvcWithDefaultRoute();           

            app.Run();
        }
    }
}