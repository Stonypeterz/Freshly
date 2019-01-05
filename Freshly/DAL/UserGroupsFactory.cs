using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Freshly.Identity {

    internal class UserGroupsFactory : BaseObj {
        
		public UserGroupsFactory() {
			Conn = new SqlConnection(Freshly.D.ConnectionString);
		}

        internal bool CheckAccess(string varUserId, string varPolicyName)
        {
            using (SqlCommand cmd = new SqlCommand("Select Count(A.[GroupName]) From dbo.[UserGroups] A Inner Join dbo.GroupPolicies B On A.GroupName = B.GroupName Where (A.[UserId] = @UserId And B.[PolicyName] = @PolicyName);", Conn))
            {
                cmd.CommandTimeout = ComTimeout;
                cmd.Parameters.Add(DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId));
                cmd.Parameters.Add(DataHelper.CreateParameter("@PolicyName", SqlDbType.NVarChar, 128, varPolicyName));
                try
                {
                    OpenConnection();
                    return (int.Parse(cmd.ExecuteScalar().ToString()) > 0);
                }
                finally
                {
                    CloseConnection();
                }
            }

        }
        
		public int AddUserToGroup(string varUserId, string varGroupName) {
			var sqlText = "If((Select A.GroupName From dbo.[UserGroups] A Where A.UserId = @UserId And A.GroupName = @GroupName) IS NULL) Insert Into dbo.[UserGroups] ([UserId], [GroupName]) Values (@UserId, @GroupName);";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId),
                DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, varGroupName)
            };
            return ExecuteCommand(sqlText, sqlParams);
		}
		public Task<int> AddUserToGroupAsync(string varUserId, string varGroupName) {
			return Task.Run(() => AddUserToGroup(varUserId, varGroupName));
		}
        
		public List<string> GetUserGroups(string UserId) {
			using (SqlCommand cmd = new SqlCommand("Select [UserId], [GroupName] From dbo.[UserGroups]", Conn)) {
				cmd.CommandTimeout = ComTimeout;
				var objList = new List<string>();
				try {
					OpenConnection();
					using (SqlDataReader dr = cmd.ExecuteReader()) {
						while (dr.Read()) {
							objList.Add((string)dr["GroupName"]);
						}
					}
					return objList;
					}
				finally {
					CloseConnection();
				}
			}

		}
		public Task<List<string>> GetRecordAsync(string UserId) {
			return Task.Run(() => GetUserGroups(UserId));
		}
        
		public bool RemoveFromGroup(string varUserId, string varGroupName){
			var sqlText = "Delete From dbo.[UserGroups] Where ([UserId] = @UserId And [GroupName] = @GroupName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId),
                DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, varGroupName)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
		}
		public Task<bool> RemoveFromGroupAsync(string varUserId, string varGroupName) {
			return Task.Run(() => RemoveFromGroup(varUserId, varGroupName));
		}

        internal bool ClearGroups(string UserId)
        {
            var sqlText = "Delete From dbo.[UserGroups] Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, UserId)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
        }

        internal bool RemoveUserFromGroup(string userId, string groupName)
        {
            var sqlText = "Delete From dbo.[UserGroups] Where ([UserId] = @UserId And [GroupName] = @GroupName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, userId),
                DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, groupName)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
        }
    }

	public class UserGroup {

		public string UserId { get; set; }
        public string GroupName { get; set; }

		public UserGroup() {
		}

        public UserGroup(string userId, string groupName)
        {
            UserId = userId;
            GroupName = groupName;
        }
    }
    
}
