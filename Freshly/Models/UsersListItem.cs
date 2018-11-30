using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    public class UsersListItem
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsLogedIn { get; set; }
        public string CurrentStatus { get; set; }
        public string OnlineStatus { get; set; }
    }
}
