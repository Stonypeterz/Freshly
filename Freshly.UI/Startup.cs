using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Freshly.Identity;
using Freshly.Identity.Models;
using Freshly.UI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Freshly.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var cfg = Configuration.GetSection("GlobalVariables");
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddFreshly(obj =>
            {
                obj.ConnectionString = Configuration.GetConnectionString("ConnStr");
                obj.TokenIssuer = cfg["ServerUrl"];
                obj.TokenAudience = cfg["ServerUrl"];
                obj.TokenKey = cfg["Tovapaky"];
                obj.PolicyType = typeof(PolicyTypes);
                obj.GroupType = typeof(GroupTypes);
                obj.DefaultUser = new ApplicationUser()
                {
                    UserId = "Stonypeterz",
                    FirstName = "Stephen",
                    LastName = "Eze",
                    Email = "e.stephen@Freshly.com",
                    PhoneNumber = "07088281148",
                    Gender = "Male",
                    Password = "Admin.1st"
                };
            });

            services.Configure<GlobalVariables>((gv) => {
                gv.CompanyName = cfg["CompanyName"];
                gv.ServerUrl = cfg["ServerUrl"];
                gv.MailServer = cfg["MailServer"];
                gv.SenderName = cfg["SenderName"];
                gv.SenderAddress = cfg["SenderAddress"];
                gv.SenderPassword = cfg["SenderPassword"];
                gv.smtpPort = int.Parse(cfg["smtpPort"]);
                gv.SupportMail = cfg["SupportMail"];
                gv.FeedBackMail = cfg["FeedBackMail"];
                gv.Tovapaky = cfg["Tovapaky"];
                gv.PageSize = int.Parse(cfg["PageSize"]);
            });

            services.AddTransient<ICustomHelpers, CustomHelpers>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
