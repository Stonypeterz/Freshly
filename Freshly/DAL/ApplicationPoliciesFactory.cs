using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Freshly.Identity {

    internal class ApplicationPoliciesFactory : BaseObj {
        
		public ApplicationPoliciesFactory() {
			Conn = new SqlConnection(Freshly.D.ConnectionString);
		}

		public bool AddAccessPolicy(string varPolicyName) {
			var sqlText = "If((Select A.PolicyName From dbo.[ApplicationPolicies] A Where A.PolicyName = @PolicyName) IS NULL) Insert Into dbo.[ApplicationPolicies] ([PolicyName]) Values (@PolicyName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@PolicyName", SqlDbType.NVarChar, 128, varPolicyName)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
		}
        
		public List<string> GetApplicationPolicies() {
			using (SqlCommand cmd = new SqlCommand("Select [PolicyName] From dbo.[ApplicationPolicies]", Conn)) {
				cmd.CommandTimeout = ComTimeout;
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
		
		public bool DeleteAccessPolicy(string varPolicyName){
			var sqlText = "Delete From dbo.[ApplicationPolicies] Where ([PolicyName] = @PolicyName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@PolicyName", SqlDbType.NVarChar, 128, varPolicyName)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
		}
	}
    
}
