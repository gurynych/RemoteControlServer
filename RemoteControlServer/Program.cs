using Microsoft.EntityFrameworkCore;
using RemoteControlServer.BusinessLogic.Cryptography;
using RemoteControlServer.BusinessLogic.Services;
using RemoteControlServer.Data;
using RemoteControlServer.Data.Interfaces;

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
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
            );
            
            builder.Services.AddMvc(mvcOtions => mvcOtions.EnableEndpointRouting = false);
            builder.Services.AddSingleton<ICryptographer, RSACryptographer>();
            builder.Services.AddSingleton<IHashCreater, BCryptCreater>();
            builder.Services.AddSingleton<ITcpListenerService, TcpListenerService>();


            //builder.Services.AddDarta();
            var app = builder.Build();

            // Configure the HTTP request pipeline.            
            app.UseStaticFiles();
            app.MapControllers();
            //app.UseAuthorization();            

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