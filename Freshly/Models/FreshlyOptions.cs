using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Freshly.Identity.Models
{
    public class FreshlyOptions
    {
        /// <summary>
        /// The default authentication scheme
        /// </summary>
        public string DefaultScheme { get; set; } = CookieAuthenticationDefaults.AuthenticationScheme;
        /// <summary>
        /// The type of web application you are trying to build
        /// </summary>
        public ApplicationTypes ApplicationType { get; set; } = ApplicationTypes.WebMVC;
        /// <summary>
        /// This is used to indicate if the system should check if a user password conforms with the password strength settings in PasswordOptions property.
        /// </summary>
        public bool CheckPasswordStrength { get; set; } = true;
        /// <summary>
        /// Connection string to SQL Server (Database)
        /// </summary>
        public string ConnectionString { get; set; } = "Server=.;Initial Catalog=FreshlyIdentity;Integrated Security=True;";
        /// <summary>
        /// Setup the password complexity (strength)
        /// </summary>
        public PasswordOptions PasswordOptions { get; set; } = new PasswordOptions();
        /// <summary>
        /// Maximum number of login atempts after consecutive failures
        /// </summary>
        public int MaxLoginAttempts { get; set; } = 5;
        /// <summary>
        /// How many minutes an account stays locked before it's automatically unlocked.
        /// </summary>
        public int LockedWindow { get; set; } = 30;
        /// <summary>
        /// Cookie settings options
        /// </summary>
        public CookieAuthenticationOptions CookieOptions { get; set; } = new CookieAuthenticationOptions()
        {
            Events = new CookieAuthenticationEvents()
            {
                OnValidatePrincipal = CookieEvents.ValidateAsync,
                OnSigningOut = CookieEvents.SignOutAsync,
                OnRedirectToLogin = CookieEvents.RedirectToLoginAsync
            },
            ClaimsIssuer = "stonypeterz.com",
            SlidingExpiration = true,
            ExpireTimeSpan = TimeSpan.FromMinutes(15),
            LoginPath = new PathString("/account/login"),
            AccessDeniedPath = new PathString("/error/accessdenied"),
            Cookie = new CookieBuilder()
            {
                Domain = "stonypeterz.com",
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            }
        };
        /// <summary>
        /// JSON Web Token (JWT) setting options
        /// </summary>
        public JwtBearerOptions JwtOptions { get; set; } = new JwtBearerOptions() {
            BackchannelTimeout = TimeSpan.FromDays(30),
            RequireHttpsMetadata = true,
            SaveToken = true
        };
        /// <summary>
        /// Setup the Initial Admin account with Group Names, Policies and all the Group Policies
        /// </summary>
        public FreshlyDefaults Defaults { get; set; } = new FreshlyDefaults()
        {
            DefaultUser = new ApplicationUser()
            {
                UserId = "Admin",
                FirstName = "Administrator",
                LastName = "System",
                Email = "admin@yourdomain.com",
                PhoneNumber = "123456789",
                Gender = "Male",
                Password = "Admin.1st",
                Groups = "Administator"
            }
        };
        
    }

}
