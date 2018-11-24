using GdevApps.BLL.Contracts;
using GdevApps.BLL.Domain;
using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.Repositories.AspNetUserRepository;
using GdevApps.DAL.Repositories.GradeBookRepository;
using GdevApps.Portal.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GdevApps.Portal.Configuration
{
    internal static class ConfigurationServices
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAspNetUserRepository, AspNetUserRepository>();
            services.AddScoped<IGradeBookRepository, GradeBookRepository>();
        }

        public static void AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAspNetUserService, AspNetUserService>();
            services.AddScoped<IGdevClassroomService, GdevClassroomService>();
            services.AddScoped<IGdevSpreadsheetService, GdevSpreadsheetService>();
            services.AddScoped<IGdevDriveService, GdevDriveService>();
        }

        public static void AddDatabaseContexts(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<AspNetUserContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Scoped, 
                ServiceLifetime.Scoped
                );

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Scoped, 
                ServiceLifetime.Scoped
                );
        }

        public static void AddAspNetIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
            })
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

        public static void AddApplicationSession(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);//You can set Time   
            });
        }

        public static void AddGdevAppsPortalLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(Log.Logger);
        }

        public static void AddGoogleAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
              .AddCookie(o => {
                  o.LoginPath = "/Account/Login";
                  })
              .AddGoogle(googleOptions =>
              {
                  googleOptions.ClientId = configuration["installed:client_id"];
                  googleOptions.ClientSecret = configuration["installed:client_secret"];
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/classroom.courses");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/classroom.coursework.students");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/classroom.profile.emails");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/classroom.profile.photos");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/classroom.rosters");
                  googleOptions.Scope.Add(" https://www.googleapis.com/auth/documents");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/drive");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/script.container.ui");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/script.scriptapp");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/script.send_mail");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/spreadsheets");
                  googleOptions.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
                  googleOptions.SaveTokens = true;
                  //googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
                  googleOptions.AccessType = "offline"; // Gets a refresh token
                  googleOptions.Events.OnCreatingTicket = ctx =>
                  {
                      //how to add new tokens
                      List<AuthenticationToken> tokens = ctx.Properties.GetTokens() as List<AuthenticationToken>;
                      tokens.Add(new AuthenticationToken()
                      {
                          Name = "created",
                          Value = DateTime.UtcNow.ToString()
                      });
                      tokens.Add(new AuthenticationToken()
                      {
                          Name = "token_updated",
                          Value = ""
                      });
                       tokens.Add(new AuthenticationToken()
                      {
                          Name = "token_updated_time",
                          Value = DateTime.MinValue.ToString()
                      });
                      ctx.Properties.StoreTokens(tokens);

                      ctx.Identity.AddClaim(new Claim("image", ctx.User.GetValue("image").SelectToken("url").ToString()));

                      return Task.CompletedTask;
                  };


                  googleOptions.AuthorizationEndpoint += "?prompt=consent";// Hack so we always get a refresh token, it only comes on the first authorization response 
              });
        }
    }
}
