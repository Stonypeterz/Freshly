using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.UI.Models
{
    public static class PolicyTypes
    {
        public const string ListUsers = "ListUsers";
        public const string DeleteUser = "DeleteUser";
        public const string CreateGroups = "CreateGroups";
        public const string DeleteGroups = "DeleteGroups";
        public const string GrantAccessToGroups = "GrantAccessToGroups";
        public const string DenyAccessToGroups = "DenyAccessToGroups";
        public const string AddUserToGroups = "AddUserToGroups";
        public const string RemoveUserFromGroups = "RemoveUserFromGroups";
        public const string ViewOtherProfiles = "ViewOtherProfiles";
        public const string EditOtherProfiles = "EditOtherProfiles";
    }
}
