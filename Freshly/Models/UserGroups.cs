using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity.Models
{
    public class UserGroups
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public List<AssignedGroups> Groups { get; set; }

        public UserGroups()
        {
            Groups = new List<AssignedGroups>();
        }
    }
}
