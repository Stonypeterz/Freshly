using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Freshly.Identity.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Freshly.Identity.DAL;

namespace Freshly.Identity
{
    public class CookieEvents
    {

        public static Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var identity = context.Principal.Identity;
            AllConsts.RPR.RemoveAll(u => u.Expires <= DateTime.UtcNow);
            if (identity?.IsAuthenticated == true)
            {
                using(var df = new ApplicationUsersFactory())
                {
                    var usr = df.GetUserByID(identity.Name);
                    if(usr.IsLogedIn == false || usr.CurrentStatus != AccountStatus.Active)
                    {
                        usr.IsLogedIn = false;
                        context.RejectPrincipal();
                        context.HttpContext.SignOutAsync();
                    }
                    usr.LastActivityDate = DateTime.UtcNow;
                    df.Update(usr);
                }
            }
            return Task.CompletedTask;
        }

        internal static Task SignInAsync(CookieSignedInContext context)
        {
            return Task.CompletedTask;
        }

        internal static Task SignOutAsync(CookieSigningOutContext context)
        {
            var id = context.HttpContext.User.Identity.Name;
            using (var df = new ApplicationUsersFactory()) {
                var usr = df.GetUser(id);
                if (usr != null) {
                    usr.IsLogedIn = false;
                    df.Update(usr);
                }
            }
            return Task.CompletedTask;
        }

        internal static Task RedirectToLoginAsync(RedirectContext<CookieAuthenticationOptions> context)
        {
            var ur = context.RedirectUri;
            if (ur?.ToLower().Contains("returnurl") == false)
                context.RedirectUri = string.Concat(ur, ur.Contains("?") ? "&ReturnUrl=" : "?ReturnUrl=");
            return Task.Run(() => context.Response.Redirect(context.RedirectUri));
        }
    }
}
