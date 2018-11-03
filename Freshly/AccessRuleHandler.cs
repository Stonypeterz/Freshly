using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Freshly.Identity
{
    public class AccessRuleHandler : AuthorizationHandler<AccessRuleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessRuleRequirement requirement)
        {
            bool granted = Freshly.CheckAccess(context.User.Identity.Name, requirement.accessPolicy);
            if (granted) context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
