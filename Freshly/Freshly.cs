﻿using Microsoft.AspNetCore.Authentication.Cookies;
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
        internal static FreshlyOptions D { get; set; }  = new FreshlyOptions();

        internal static bool CheckAccess(string usrName, string accessRule)
        {
            bool granted = false;
            using (var ur = new UserGroupsFactory())
            {
                granted = ur.CheckAccess(usrName, accessRule);
            }
            return granted;
        }

        public static AuthenticationBuilder AddFreshly(this AuthenticationBuilder builder, Action<FreshlyOptions> defaults)
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

        public static IServiceCollection AddFreshly(this IServiceCollection services, AuthScheme scheme, Action<FreshlyOptions> defaults)
        {
            defaults?.Invoke(D);
            SetDefaults(D);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
                .AddTransient<IAuthorizationHandler, AccessRuleHandler>()
                .AddTransient<UserManager>()
                .AddTransient<GroupManager>();
                
            if(scheme == AuthScheme.Cookie)
            {
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(option =>
                    {
                        option.LoginPath = new PathString("/account/login");
                        option.AccessDeniedPath = new PathString("/error/accessdenied");
                        option.SlidingExpiration = true;
                        option.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                        option.Cookie = new CookieBuilder()
                        {
                            Domain = D.CookieDomain
                        };
                        option.Events.OnValidatePrincipal = CookieEvents.ValidateAsync;
                    });
                services.AddAuthorization(options =>
                {
                    if (D.Policies.Count > 0)
                    {
                        foreach (var ar in D.Policies)
                        {
                            options.AddPolicy(ar, policy => {
                                policy.RequireAuthenticatedUser();
                                policy.Requirements.Add(new AccessRuleRequirement(ar));
                                //policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme);
                            });
                        }
                    }
                });
            }
            else if (scheme == AuthScheme.JwtBearer)
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                    });
                services.AddAuthorization(options =>
                {
                    if (D.Policies.Count > 0)
                    {
                        foreach (var ar in D.Policies)
                        {
                            options.AddPolicy(ar, policy => {
                                policy.RequireAuthenticatedUser();
                                policy.Requirements.Add(new AccessRuleRequirement(ar));
                                //policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                            });
                        }
                    }
                });
            }
            else if (scheme == AuthScheme.Both)
            {
                services.AddAuthentication()//CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(option =>
                    {
                        option.LoginPath = new PathString("/account/login");
                        option.AccessDeniedPath = new PathString("/error/accessdenied");
                        option.SlidingExpiration = true;
                        option.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                        option.Cookie = new CookieBuilder()
                        {
                            HttpOnly = true,
                            Domain = D.CookieDomain
                        };
                        option.Events.OnValidatePrincipal = CookieEvents.ValidateAsync;
                    })
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
                    });
                services.AddAuthorization(options =>
                {
                    //var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    //CookieAuthenticationDefaults.AuthenticationScheme,
                    //JwtBearerDefaults.AuthenticationScheme);
                    //defaultAuthorizationPolicyBuilder = defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                    //options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();

                    if (D.Policies.Count > 0)
                    {
                        foreach (var ar in D.Policies)
                        {
                            options.AddPolicy(ar, policy => {
                                policy.RequireAuthenticatedUser();
                                policy.Requirements.Add(new AccessRuleRequirement(ar));
                                policy.AddAuthenticationSchemes(CookieAuthenticationDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme);
                            });
                        }
                    }
                });
            }

            return services;
        }

        public static IServiceCollection AddFreshly(this IServiceCollection services, Action<FreshlyOptions> defaults)
        {
            return services.AddFreshly(AuthScheme.Both, defaults);
        }

        internal static void SetDefaults(FreshlyOptions D)
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
    
    public enum AuthScheme
    {
        Cookie,
        JwtBearer,
        Both
    }
}
