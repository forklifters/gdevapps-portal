using GdevApps.BLL.Contracts;
using GdevApps.BLL.Domain;
using GdevApps.DAL.DataContexts.AspNetUsers;
using GdevApps.DAL.Repositories.AspNetUserRepository;
using GdevApps.Portal.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public static void AddAspNetIdentity(this IServiceCollection services)
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

        public static void AddGoogleAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication()
              .AddGoogle(googleOptions =>
              {
                  googleOptions.ClientId = "898218061018-1mvqrmk07v8206bhsdmf8cs3kkd7rni9.apps.googleusercontent.com";
                  googleOptions.ClientSecret = "5eE60z31j9J7y2vQvYQx68kK";
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
                  googleOptions.ClaimActions.MapJsonKey(ClaimTypes.Gender, "gender");
                  googleOptions.Events.OnCreatingTicket = ctx =>
                  {
                      List<AuthenticationToken> tokens = ctx.Properties.GetTokens() as List<AuthenticationToken>;
                      tokens.Add(new AuthenticationToken()
                      {
                          Name = "TicketCreated",
                          Value = DateTime.UtcNow.ToString()
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
