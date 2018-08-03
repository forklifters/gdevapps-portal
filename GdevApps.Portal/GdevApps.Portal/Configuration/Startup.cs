using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using GdevApps.Portal.Data;
using GdevApps.Portal.Services;
using GdevApps.Portal.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GdevApps.Portal
{
    public class Startup
    {
        private string _clietId = null;
        private string _clientSecret = null;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        //public Startup(IHostingEnvironment env)
        //{
        //    var builder = new ConfigurationBuilder()
        //.SetBasePath(env.ContentRootPath)
        //.AddJsonFile("appsettings.json",
        //             optional: false,
        //             reloadOnChange: true)
        //.AddEnvironmentVariables();

        //var builder = new ConfigurationBuilder()
        //                    .SetBasePath(env.ContentRootPath)
        //                    .AddJsonFile("Configuration/appsettings.json", optional: true, reloadOnChange: true)
        //                    .AddJsonFile("Configuration/ConnectionStrings.json", optional: true)
        //                    .AddEnvironmentVariables();
        //Configuration = builder.Build();

        //    if (env.IsDevelopment())
        //    {
        //        builder.AddUserSecrets<Startup>();
        //    }

        //    Configuration = builder.Build();
        //}

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(config => {
                var policy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();

                config.Filters.Add(new AuthorizeFilter(policy));
                config.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });
            services.AddDatabaseContexts(Configuration);
            services.AddAspNetIdentity(Configuration);
            services.AddRepositories();
            services.AddDomainServices();
            services.AddSingleton(AutoMapperConfiguration.MapperConfiguration.CreateMapper());

            //TEST
            services.ConfigureApplicationCookie(options =>
            {
                //Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                // If the LoginPath isn't set, ASP.NET Core defaults 
                // the path to /Account/Login.
                options.LoginPath = "/Account/Login";
                // If the AccessDeniedPath isn't set, ASP.NET Core defaults 
                // the path to /Account/AccessDenied.
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();


            // services.Configure<MySecrets>(Configuration.GetSection(nameof(MySecrets))).AddOptions().BuildServiceProvider();

            _clietId = "898218061018-1mvqrmk07v8206bhsdmf8cs3kkd7rni9.apps.googleusercontent.com";
            _clientSecret = "5eE60z31j9J7y2vQvYQx68kK";
            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = _clietId;
                    googleOptions.ClientSecret = _clientSecret;
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
                        tokens.Add(new AuthenticationToken() { Name = "TicketCreated", Value = DateTime.UtcNow.ToString() });
                        ctx.Properties.StoreTokens(tokens);
                        return Task.CompletedTask;
                    };


                    googleOptions.AuthorizationEndpoint += "?prompt=consent";// Hack so we always get a refresh token, it only comes on the first authorization response 
                });
                
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
