using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Freshly.Identity.Models;

namespace Freshly.Identity {

    internal class GroupPoliciesFactory : BaseObj {
        
		public GroupPoliciesFactory() {
			Conn = new SqlConnection(Freshly.D.ConnectionString);
		}

		public bool AddGroupPolicy(string varGroupName, string varPolicyName) {
			var sqlText = "If((Select A.PolicyName From dbo.[GroupPolicies] A Where A.GroupName = @GroupName And A.PolicyName = @PolicyName) IS NULL) Insert Into dbo.[GroupPolicies] ([GroupName], [PolicyName]) Values (@GroupName, @PolicyName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, varGroupName),
                DataHelper.CreateParameter("@PolicyName", SqlDbType.NVarChar, 128, varPolicyName)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
		}

		public bool AddGroupRule(GroupPolicy obj) {
			return AddGroupPolicy(obj.GroupName, obj.PolicyName);
		}

		public List<string> GetGroupPolicies(string GroupName) {
			using (SqlCommand cmd = new SqlCommand("Select [PolicyName] From dbo.[GroupPolicies] Where [GroupName] = @GroupName", Conn)) {
				cmd.CommandTimeout = ComTimeout;
                cmd.Parameters.Add("@GroupName", SqlDbType.NVarChar, 128).Value = GroupName;
                var objList = new List<string>();
				try {
					OpenConnection();
					using (SqlDataReader dr = cmd.ExecuteReader()) {
						while (dr.Read()) {
							objList.Add((string)dr["PolicyName"]);
						}
					}
					return objList;
					}
				finally {
					CloseConnection();
				}
			}

		}
		
		public int DeleteGroupRule(string varGroupName, string varPolicyName){
			var sqlText = "Delete From dbo.[GroupPolicies] Where ([GroupName] = @GroupName And [PolicyName] = @PolicyName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, varGroupName),
                DataHelper.CreateParameter("@PolicyName", SqlDbType.NVarChar, 128, varPolicyName)
            };
            return ExecuteCommand(sqlText, sqlParams);
		}
	}

}
