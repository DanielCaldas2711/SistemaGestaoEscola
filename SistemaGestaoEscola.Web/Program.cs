using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SistemaGestaoEscola.Web.Data;
using SistemaGestaoEscola.Web.Data.Entities;
using SistemaGestaoEscola.Web.Helpers;
using SistemaGestaoEscola.Web.Helpers.Interfaces;


namespace SistemaGestaoEscola.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region DataContext

            //Get the connection string
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


            //Add DbContext using Sql Server
            builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

            #endregion

            // Add services to the container.            

            builder.Services.AddControllersWithViews();

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

            //Password rules configuration

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            });

            //Services

            builder.Services.AddTransient<SeedDb>();

            builder.Services.AddScoped<IUserHelper, UserHelper>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            RunSeeding(app); //Runs SeedDb

            app.Run();

        }
        private static void RunSeeding(IHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetService<SeedDb>();
                seeder.SeedAsync().Wait();
            }
        }
    }
}
