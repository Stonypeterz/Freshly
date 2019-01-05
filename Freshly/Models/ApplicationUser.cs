using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    public class ApplicationUser
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; } = null;
        public string Gender { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } = null;
        public string CurrentStatus { get; internal set; } = AccountStatus.Pending;
        public string OnlineStatus { get; internal set; } = "Available";
        public string Password { get; set; }
        public string Groups { get; internal set; } = "";
        public bool IsActive { get { return CurrentStatus == AccountStatus.Active; } }
        public bool IsLogedIn { get; internal set; } = false;
        public DateTime? LastLoginDate { get; internal set; } = null;
        public DateTime? LastLockDate { get; internal set; } = null;
        public DateTime LastActivityDate { get; internal set; }
        public DateTime RegDate { get; internal set; }
        
        public ApplicationUser() { }
        public ApplicationUser(bool activate) {
            if (activate) CurrentStatus = AccountStatus.Active;
        }
    }
}

