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

	internal class ApplicationGroupsFactory : BaseObj {
        
		public ApplicationGroupsFactory() {
			Conn = new SqlConnection(Freshly.D.ConnectionString);
		}

		public bool AddGroup(string varGroupName) {
			var sqlText = "If((Select A.GroupName From dbo.[ApplicationGroups] A Where A.GroupName = @GroupName) IS NULL) Insert Into dbo.[ApplicationGroups] ([GroupName]) Values (@GroupName)";
			var sqlParams = new List<SqlParameter>();
			sqlParams.Add(DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, varGroupName));
			return ExecuteCommand(sqlText, sqlParams) > 0;
		}
        
		public List<string> GetRecord() {
			using (SqlCommand cmd = new SqlCommand("Select [GroupName] From dbo.[ApplicationGroups]", Conn)) {
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
		
		public bool DeleteGroup(string varGroupName){
			var sqlText = "Delete From dbo.[ApplicationGroups] Where ([GroupName] = @GroupName)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@GroupName", SqlDbType.NVarChar, 128, varGroupName)
            };
            return ExecuteCommand(sqlText, sqlParams) > 0;
		}
	}
    
}
