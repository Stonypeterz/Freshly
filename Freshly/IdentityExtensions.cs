using System;
using System.Linq;
using System.Security.Claims;

namespace Freshly.Identity
{
    public static class IdentityExtensions {

        public static bool HasRightTo(this ClaimsPrincipal usr, string accessPolicy)
        {
            if (!usr.Identity.IsAuthenticated) return false;
            return Freshly.CheckAccess(usr.Identity.Name, accessPolicy);
        }

        public static bool AddToGroup(this ClaimsPrincipal usr, string groupName)
        {
            if (!usr.Identity.IsAuthenticated) return false;
            using (var ug = new UserGroupsFactory())
            {
                ug.AddUserToGroup(usr.Identity.Name, groupName);
            };
            return true;
        }

        public static bool IsInGroup(this ClaimsPrincipal usr, string groupName)
        {
            if (!usr.Identity.IsAuthenticated) return false;
            bool? f = false;
            using (var ug = new UserGroupsFactory())
            {
                f = ug.GetUserGroups(usr.Identity.Name)?.Contains(groupName);
            };
            return f ?? false;
        }

        public static string GetFirstName(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                var fn = user.Claims.First(s => s.Type.ToLower().Contains("given_name"));
                return fn?.Value;
            }
            return "";
        }

        public static string GetLastName(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                var ln = user.Claims.First(s => s.Type.ToLower().Contains("family_name"));
                return ln?.Value;
            }
            return "";
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                var fn = user.Claims.FirstOrDefault(s => s.Type.ToLower().Contains("given_name"));
                var ln = user.Claims.FirstOrDefault(s => s.Type.ToLower().Contains("family_name"));
                return $"{fn?.Value} {ln?.Value}";
            }
            return "";
        }

        public static string GetGender(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated) {
                var fn = user.Claims.First(s => s.Type.ToLower().Contains("gender"));
                return fn?.Value;
            }
            return "";
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated) {
                var fn = user.Claims.First(s => s.Type.ToLower().Contains("emailaddress"));
                return fn?.Value;
            }
            return "";
        }

        public static bool SetOnlineStatus(this ClaimsPrincipal usr, string status)
        {
            if (status.Length > 16) throw new Exception("The [status] must not be more than 16 characters long");
            var f = (new ApplicationUsersFactory()).SetOnlineStatus(usr.Identity.Name, status);
            return f;
        }

    }

}
