using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Freshly.Identity
{
    public class GroupManager : IDisposable
    {
        private ApplicationGroupsFactory AGF { get; set; }
        private GroupPoliciesFactory GPF { get; set; }
        private ApplicationPoliciesFactory APF { get; set; }

        public GroupManager()
        {
            AGF = new ApplicationGroupsFactory();
            GPF = new GroupPoliciesFactory();
            APF = new ApplicationPoliciesFactory();
        }

        public Task AddGroupAsync(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(groupName);
            AGF.AddGroup(groupName);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetGroupsAsync()
        {
            var gs = AGF.GetRecord();
            return Task.FromResult(gs);
        }

        public Task AddGroupsAsync(string[] groupNames)
        {
            if (groupNames == null) throw new ArgumentNullException(nameof(groupNames));
            foreach (var s in groupNames)if (!string.IsNullOrEmpty(s)) AGF.AddGroup(s);
            return Task.CompletedTask;
        }

        public Task DeleteGroupAsync(string groupName)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(groupName);
            AGF.DeleteGroup(groupName);
            return Task.CompletedTask;
        }

        [Obsolete("GrantAccessAsync was a bad naming choice and has been changed. It will be removed in the next version. Please use GrantPolicyRightToGroupAsync", false)]
        public Task GrantAccessAsync(string groupName, string accessPolicy)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrEmpty(accessPolicy)) throw new ArgumentNullException(nameof(accessPolicy));
            GPF.AddGroupPolicy(groupName, accessPolicy);
            return Task.CompletedTask;
        }

        public Task GrantPolicyRightToGroupAsync(string groupName, string accessPolicy)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrEmpty(accessPolicy)) throw new ArgumentNullException(nameof(accessPolicy));
            GPF.AddGroupPolicy(groupName, accessPolicy);
            return Task.CompletedTask;
        }

        [Obsolete("DenyAccessAsync was a bad naming choice and has been changed. It will be removed in the next version. Please use DenyPolicyRightToGroupAsync", false)]
        public Task DenyAccessAsync(string groupName, string accessPolicy)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrEmpty(accessPolicy)) throw new ArgumentNullException(nameof(accessPolicy));
            GPF.DeleteGroupRule(groupName, accessPolicy);
            return Task.CompletedTask;
        }

        public Task DenyPolicyRightToGroupAsync(string groupName, string accessPolicy)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrEmpty(accessPolicy)) throw new ArgumentNullException(nameof(accessPolicy));
            GPF.DeleteGroupRule(groupName, accessPolicy);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetGroupPolicyRightsAsync(string groupName)
        {
            var gps = GPF.GetGroupPolicies(groupName);
            return Task.FromResult(gps);
        }

        public Task<List<string>> GetAllPoliciesAsync()
        {
            var aps = Freshly.D.Policies; //APF.GetApplicationPolicies();
            return Task.FromResult(aps);
        }

        public void Dispose()
        {
            AGF.Dispose();
            GPF.Dispose();
            APF.Dispose();
        }
    }
}
