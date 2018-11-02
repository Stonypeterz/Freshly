using Freshly.Identity.DAL;
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

        public GroupManager()
        {
            AGF = new ApplicationGroupsFactory();
            GPF = new GroupPoliciesFactory();
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

        public Task GrantAccessAsync(string groupName, string accessPolicy)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrEmpty(accessPolicy)) throw new ArgumentNullException(nameof(accessPolicy));
            GPF.AddGroupPolicy(groupName, accessPolicy);
            return Task.CompletedTask;
        }

        public Task DenyAccessAsync(string groupName, string accessPolicy)
        {
            if (string.IsNullOrEmpty(groupName)) throw new ArgumentNullException(nameof(groupName));
            if (string.IsNullOrEmpty(accessPolicy)) throw new ArgumentNullException(nameof(accessPolicy));
            GPF.DeleteGroupRule(groupName, accessPolicy);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            AGF.Dispose();
            GPF.Dispose();
        }
    }
}
