using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Freshly.Identity
{
    public class AccessRuleRequirement : IAuthorizationRequirement
    {

        internal string accessPolicy;

        public AccessRuleRequirement(string __accessPolicy)
        {
            accessPolicy = __accessPolicy;
        }

    }
}
