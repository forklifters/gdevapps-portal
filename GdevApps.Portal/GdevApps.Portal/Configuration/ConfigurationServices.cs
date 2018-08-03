using GdevApps.BLL.Contracts;
using GdevApps.BLL.Domain;
using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.Repositories.AspNetUserRepository;
using GdevApps.Portal.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GdevApps.Portal.Configuration
{
    internal static class ConfigurationServices
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAspNetUserRepository, AspNetUserRepository>();
        }

        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAspNetUserService, AspNetUserService>();
        }

        public static void AddDatabaseContexts(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<AspNetUserContext>(options =>
                //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"))
                );

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"))
                );
        }

        public static void AddAspNetIdentity(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                //Password
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 1;
                //Lockout settins
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                //User settings
                options.User.RequireUniqueEmail = true;
            });
        }

        public static void AddApplicationLogging(this IServiceCollection services)
        {
            // services.AddSingleton(Log.Logger);
        }
    }
}
