using Freshly.Identity.DAL;
using Freshly.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Freshly.Identity.Models
{
    public class FreshlyDefaults
    {

        public string TokenIssuer { get; set; }
        public string TokenAudience { get; set; }
        public string TokenKey { get; set; }
        public string ConnectionString { get; set; } = "Server=.;Initial Catalog=FreshlyIdentity;Integrated Security=True;";
        public PasswordOptions PasswordOptions { get; set; } = new PasswordOptions();
        public bool CheckPasswordStrength { get; set; } = true;
        public double LockedWindow { get; set; } = 30;
        public int MaxLoginAttempts { get; set; } = 5;
        public ApplicationUser DefaultUser { get; set; }
        public Type GroupType { private get; set; }
        public Type PolicyType { private get; set; }

        internal List<string> Groups
        {
            get {
                if (GroupType != null)
                {
                    var n = from m in GroupType.GetFields() select m.Name;
                    return n.ToList();
                }
                return new List<string>();
            }
        }

        internal List<string> Policies
        {
            get {
                if (PolicyType != null)
                {
                    var n = from m in PolicyType.GetFields() select m.Name;
                    return n.ToList();
                }
                return new List<string>();
            }
        }

        public FreshlyDefaults()
        {

        }
    }
}
