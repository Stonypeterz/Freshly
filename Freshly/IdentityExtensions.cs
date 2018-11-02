using Freshly.Identity.DAL;
using System.Linq;
using System.Security.Claims;

namespace Freshly.Identity
{
    public static class IdentityExtensions {

        public static bool AdheresTo(this ClaimsPrincipal usr, string accessPolicy)
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
                var fn = user.Claims.First(s => s.Type == "FirstName");
                return fn?.Value;
            }
            return "";
        }

        public static string GetLastName(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                var ln = user.Claims.First(s => s.Type == "LastName");
                return ln?.Value;
            }
            return "";
        }

        public static string GetFullName(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                var fn = user.Claims.FirstOrDefault(s => s.Type == "FirstName");
                var ln = user.Claims.FirstOrDefault(s => s.Type == "LastName");
                return $"{fn?.Value} {ln?.Value}";
            }
            return "";
        }

        public static string GetGender(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated) {
                var fn = user.Claims.First(s => s.Type == "Gender");
                return fn?.Value;
            }
            return "";
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated) {
                var fn = user.Claims.First(s => s.Type == "Email");
                return fn?.Value;
            }
            return "";
        }

    }

}
