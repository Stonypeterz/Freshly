using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Freshly.Identity;
using Freshly.Identity.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Freshly.Identity
{
    public static class Freshly
    {
        private static List<string> LstPolicy = new List<string>();
        internal static FreshlyDefaults D { get; set; }  = new FreshlyDefaults();

        internal static bool CheckAccess(string usrName, string accessRule)
        {
            bool granted = false;
            using (var ur = new UserGroupsFactory())
            {
                granted = ur.CheckAccess(usrName, accessRule);
            }
            return granted;
        }

        public static AuthenticationBuilder AddFreshly(this AuthenticationBuilder builder, Action<FreshlyDefaults> defaults)
        {
            defaults?.Invoke(D);
            SetDefaults(D);

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddTransient<IAuthorizationHandler, AccessRuleHandler>()
            .AddTransient<UserManager>()
            .AddTransient<GroupManager>()
            .AddAuthorization(options =>
            {
                if (D.Policies.Count > 0)
                {
                    foreach (var ar in D.Policies)
                    {
                        options.AddPolicy(ar, policy =>
                        {
                            policy.Requirements.Add(new AccessRuleRequirement(ar));
                            //policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                        });
                    }
                }
            });

            return builder;
        }

        public static IServiceCollection AddFreshly(this IServiceCollection services, Action<FreshlyDefaults> defaults)
        {
            defaults?.Invoke(D);
            SetDefaults(D);

            return services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddTransient<IAuthorizationHandler, AccessRuleHandler>()
                .AddTransient<UserManager>()
                .AddTransient<GroupManager>()
                .AddAuthorization(options =>
                {
                    if (D.Policies.Count > 0)
                    {
                        foreach (var ar in D.Policies)
                        {
                            options.AddPolicy(ar, policy => {
                                policy.Requirements.Add(new AccessRuleRequirement(ar));
                                //policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                            });
                        }
                    }
                })
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    option.RequireHttpsMetadata = true;
                    option.SaveToken = true;
                    option.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = D.TokenIssuer,
                        ValidAudience = D.TokenAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(D.TokenKey))
                    };
                })
                .AddCookie(option =>
                {
                    option.SlidingExpiration = true;
                    option.Events.OnValidatePrincipal = CookieEvents.ValidateAsync;
                })
                .Services;
        }

        internal static void SetDefaults(FreshlyDefaults D)
        {
            var AG = new ApplicationGroupsFactory();
            var AP = new ApplicationPoliciesFactory();
            var UG = new UserGroupsFactory();
            var GP = new GroupPoliciesFactory();
            var AU = new UserHelpers();

            var gs = D?.Groups;
            var ps = D?.Policies;
            var AdminGroup = "Administrator";

            (new DatabaseInitializer()).SetupDB();

            try { AG.AddGroup(AdminGroup); } catch(Exception ex) { };
            try {
                if (D?.DefaultUser != null) {
                    D.DefaultUser.CurrentStatus = AccountStatus.Active;
                    AU.CreateUser(D.DefaultUser, D.DefaultUser.Password);
                    UG.AddUserToGroup(D.DefaultUser.UserId, AdminGroup);
                }
            } catch (Exception ex) { }
            
            try { if (gs != null) foreach (var s in gs) if (!string.IsNullOrEmpty(s)) AG.AddGroup(s); } catch (Exception ex) { }
            try { if (ps != null) foreach (var s in ps) { if (!string.IsNullOrEmpty(s)) { AP.AddAccessPolicy(s); GP.AddGroupPolicy(AdminGroup, s); } } } catch (Exception ex) { }

            try {
                if (!string.IsNullOrEmpty(D?.DefaultUser.Groups))
                {
                    var ug = D?.DefaultUser?.Groups.Split(',');
                    var un = D?.DefaultUser.UserId;
                    foreach (var g in ug) UG.AddUserToGroup(un, g);
                }
            } catch (Exception ex) { }
        
            try
            {
                //Create policies for all the access Policies
                LstPolicy = AP.GetApplicationPolicies();
            }
            catch (Exception ex) { }

            AP.Dispose(); AG.Dispose(); AU.Dispose(); GP.Dispose(); UG.Dispose();

            //AuthB.Services.AddDataProtection()
            //    .PersistKeysToFileSystem(new DirectoryInfo(@""))
            //    .ProtectKeysWithCertificate("thumbnail")
            //    .SetApplicationName("Freshly");

        }

    }
    
}
