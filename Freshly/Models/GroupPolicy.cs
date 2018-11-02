using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Freshly.Identity.Models
{

    public class GroupPolicy
    {

        public string GroupName { get; set; }
        public string PolicyName { get; set; }

        public GroupPolicy()
        {
        }

        public GroupPolicy(string groupName, string policyName)
        {
            GroupName = groupName;
            PolicyName = policyName;
        }
    }

}
