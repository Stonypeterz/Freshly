using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public string Password { get; set; }
        public string Groups { get; internal set; } = "";
        public string Status { get { return CurrentStatus; } }
        public bool IsActive { get { return CurrentStatus == AccountStatus.Active; } }
        internal bool IsLogedIn { get; set; } = false;
        internal DateTime? LastLoginDate { get; set; } = null;
        internal string CurrentStatus { get; set; } = AccountStatus.Pending;
        internal DateTime? LastLockDate { get; set; } = null;
        internal DateTime LastActivityDate { get; set; }
        internal DateTime RegDate { get; set; }
        
        public ApplicationUser() { }
        public ApplicationUser(bool activate) {
            if (activate) CurrentStatus = AccountStatus.Active;
        }
    }
}

