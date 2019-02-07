using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Security.Claims;
using Freshly.Identity.Models;

namespace Freshly.Identity {

    internal class ApplicationUsersFactory : BaseObj {

		private int TotalPages;

		public ApplicationUsersFactory() {
			Conn = new SqlConnection(Freshly.D.ConnectionString);
		}
        
        public int Create(ApplicationUser obj) {
			var sqlText = "Insert Into dbo.[ApplicationUsers] ([UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [AccountStatus], [OnlineStatus]) Values (@UserId, @FirstName, @LastName, @DateOfBirth, @Gender, @Email, @PhoneNumber, @Password, @AccountStatus, @OnlineStatus)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, obj.UserId),
                DataHelper.CreateParameter("@FirstName", SqlDbType.NVarChar, 256, obj.FirstName),
                DataHelper.CreateParameter("@LastName", SqlDbType.NVarChar, 256, obj.LastName),
                DataHelper.CreateParameter("@DateOfBirth", SqlDbType.DateTime, 8, obj.DateOfBirth, true),
                DataHelper.CreateParameter("@Gender", SqlDbType.NVarChar, 128, obj.Gender),
                DataHelper.CreateParameter("@Email", SqlDbType.NVarChar, 256, obj.Email),
                DataHelper.CreateParameter("@AccountStatus", SqlDbType.NVarChar, 256, obj.CurrentStatus),
                DataHelper.CreateParameter("@OnlineStatus", SqlDbType.NVarChar, 16, obj.OnlineStatus),
                DataHelper.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, 128, obj.PhoneNumber, true),
                DataHelper.CreateParameter("@Password", SqlDbType.NVarChar, -1, obj.Password)
            };
            return ExecuteCommand(sqlText, sqlParams);
		}
        
        public UsersPage GetPage(string GroupName, int PageNo, int PageSize = 10, string filterQ = "0")
        {
            using (SqlCommand cmd = new SqlCommand("[dbo].[GetUsersList]", Conn))
            {
                cmd.CommandTimeout = ComTimeout;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PageNo", PageNo);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);
                cmd.Parameters.AddWithValue("@GroupName", string.IsNullOrEmpty(GroupName) == true ? "All" : GroupName);
                cmd.Parameters.AddWithValue("@filterQ", string.IsNullOrEmpty(filterQ) == true ? "0" : filterQ);
                List<UsersListItem> objList = new List<UsersListItem>();
                UsersListItem obj = null;
                try
                {
                    OpenConnection();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            obj = new UsersListItem
                            {
                                UserId = (string)dr["UserId"],
                                FirstName = (string)dr["FirstName"],
                                LastName = (string)dr["LastName"],
                                IsLogedIn = (bool)dr["IsLogedIn"],
                                CurrentStatus = (string)dr["AccountStatus"],
                                OnlineStatus = (string)dr["OnlineStatus"]
                            };

                            objList.Add(obj);
                        }
                        dr.NextResult();
                        while (dr.Read())
                        {
                            TotalPages = (int)dr["PageCount"];
                        }
                    }
                    return new UsersPage(objList, PageNo, PageSize, TotalPages, filterQ);
                }
                finally
                {
                    CloseConnection();
                }
            }

        }

        public void SetLogedIn(bool status, ApplicationUser usr)
        {
            if (usr != null)
            {
                SetLogedIn(status, usr.UserId);
            }
        }

        public int SetLogedIn(bool status, string usrName)
        {
            var sqlText = $"Update dbo.[ApplicationUsers] Set [IsLogedIn] = @IsLogedIn, {(status == true ? "[LastLoginDate] = @LastLoginDate, " : "")}[LastActivityDate] = GetDate() Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, usrName),
                DataHelper.CreateParameter("@IsLogedIn", SqlDbType.Bit, 1, status),
            };
            if (status) sqlParams.Add(DataHelper.CreateParameter("@LastLoginDate", SqlDbType.DateTime, 8, DateTime.UtcNow));
            return ExecuteCommand(sqlText, sqlParams);
        }

        public bool SetOnlineStatus(string usrName, string status)
        {
            var sqlText = "Update dbo.[ApplicationUsers] Set [OnlineStatus] = @OnlineStatus, [LastActivityDate] = GetDate() Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, usrName),
                DataHelper.CreateParameter("@OnlineStatus", SqlDbType.NVarChar, 16, status),
            };
            var i = ExecuteCommand(sqlText, sqlParams);
            return i == 0 ? false : true;
        }

        internal bool ChangeStatus(string userId, string status)
        {
            var sqlText = $"Update dbo.[ApplicationUsers] Set [AccountStatus] = @AccountStatus{(status == Status.Locked.ToString() ? ", [LastLockDate] = GetDate()" : "")} Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, userId),
                DataHelper.CreateParameter("@AccountStatus", SqlDbType.NVarChar, 16, status),
            };
            var i = ExecuteCommand(sqlText, sqlParams);
            return i == 0 ? false : true;
        }

        private ApplicationUser GetRecordByID(string varSql, List<SqlParameter> sqlParams) {
			using (SqlCommand cmd = new SqlCommand($"Select [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [IsLogedIn], [LastLoginDate], [AccountStatus], [OnlineStatus], [LastLockDate], [LastActivityDate], [RegDate] From dbo.[ApplicationUsers] Where {varSql}", Conn)) {
				cmd.CommandTimeout = ComTimeout;
				cmd.Parameters.AddRange(sqlParams.ToArray());

				ApplicationUser obj = null;
				try {
					OpenConnection();
					using (SqlDataReader dr = cmd.ExecuteReader()) {
						while (dr.Read()) {
                            obj = new ApplicationUser
                            {
                                UserId = (string)dr["UserId"],
                                FirstName = (string)dr["FirstName"],
                                LastName = (string)dr["LastName"],
                                DateOfBirth = (dr["DateOfBirth"] == System.DBNull.Value) ? null : (DateTime?)dr["DateOfBirth"],
                                Gender = (string)dr["Gender"],
                                Email = (string)dr["Email"],
                                PhoneNumber = (dr["PhoneNumber"] == System.DBNull.Value) ? null : (string)dr["PhoneNumber"],
                                Password = (string)dr["Password"],
                                IsLogedIn = (bool)dr["IsLogedIn"],
                                LastLoginDate = (dr["LastLoginDate"] == System.DBNull.Value) ? null : (DateTime?)dr["LastLoginDate"],
                                CurrentStatus = (string)dr["AccountStatus"],
                                OnlineStatus = (string)dr["OnlineStatus"],
                                LastLockDate = (dr["LastLockDate"] == System.DBNull.Value) ? null : (DateTime?)dr["LastLockDate"],
                                LastActivityDate = (DateTime)dr["LastActivityDate"],
                                RegDate = (DateTime)dr["RegDate"]
                            };

                        }
                        if(obj != null)
                        {
                            int countr = 0;
                            dr.NextResult();
                            while (dr.Read())
                            {
                                if (countr == 0) obj.Groups = (string)dr["GroupName"];
                                else obj.Groups += $",{(string)dr["GroupName"]}";
                                countr++;
                            }
                        }
					}
					return obj;
				}
				finally {
					CloseConnection();
				}
			}

		}

        public ApplicationUser GetUserByID(string varUserId)
        {
            string sql = "([UserId] = @UserId); Select A.GroupName From dbo.UserGroups A Where A.UserId = @UserId;";
            var lst = new List<SqlParameter>()
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId)
            };
            return GetRecordByID(sql, lst);
        }
        
        public ApplicationUser GetUser(string varUserId)
        {
            string sql = "([UserId] = @UserId OR [Email] = @Email OR [PhoneNumber] = @PhoneNumber); Select A.GroupName From dbo.UserGroups A Inner Join dbo.ApplicationUsers B On A.UserId = B.UserId Where A.[UserId] = @UserId OR B.[Email] = @Email OR B.[PhoneNumber] = @PhoneNumber;";
            var lst = new List<SqlParameter>()
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId),
                DataHelper.CreateParameter("@Email", SqlDbType.NVarChar, 256, varUserId),
                DataHelper.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, 128, varUserId)
            };
            return GetRecordByID(sql, lst);
        }

        public UserAssignedGroups GetGroups(string UserId)
        {
            using (SqlCommand cmd = new SqlCommand("Select UserId, FirstName, LastName From dbo.ApplicationUsers Where UserId = @UserId; Select A.GroupName, Case When B.UserId IS NULL Then 'false' Else 'true' End As IsAssigned From dbo.ApplicationGroups A Left Outer Join dbo.UserGroups B On A.GroupName = B.GroupName And B.UserId = @UserId;", Conn)) {
                cmd.CommandTimeout = ComTimeout;
                cmd.Parameters.AddWithValue("@UserId", UserId);
                var ug = new UserAssignedGroups();
                AssignedGroups obj = null;
                try {
                    OpenConnection();
                    using (SqlDataReader dr = cmd.ExecuteReader()) {
                        while (dr.Read()) {
                            ug.UserId = (string)dr["UserId"];
                            ug.FullName = $"{(string)dr["FirstName"]} {(string)dr["LastName"]}";
                        }
                        dr.NextResult();
                        while (dr.Read()) {
                            obj = new AssignedGroups {
                                GroupName = (string)dr["GroupName"],
                                IsAssigned = bool.Parse(dr["IsAssigned"].ToString())
                            };
                            ug.Groups.Add(obj);
                        }
                    }
                    return ug;
                } finally {
                    CloseConnection();
                }
            }
        }

        public int Update(ApplicationUser obj) {
			var sqlText = "Update dbo.[ApplicationUsers] Set [FirstName] = @FirstName, [LastName] = @LastName, [DateOfBirth] = @DateOfBirth, [Gender] = @Gender, [Email] = @Email, [PhoneNumber] = @PhoneNumber, [Password] = @Password, [IsLogedIn] = @IsLogedIn, [LastLoginDate] = @LastLoginDate, [AccountStatus] = @AccountStatus, [OnlineStatus] = @OnlineStatus, [LastLockDate] = @LastLockDate, [LastActivityDate] = GetDate() Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, obj.UserId),
                DataHelper.CreateParameter("@FirstName", SqlDbType.NVarChar, 256, obj.FirstName),
                DataHelper.CreateParameter("@LastName", SqlDbType.NVarChar, 256, obj.LastName),
                DataHelper.CreateParameter("@DateOfBirth", SqlDbType.DateTime, 8, obj.DateOfBirth, true),
                DataHelper.CreateParameter("@Gender", SqlDbType.NVarChar, 128, obj.Gender),
                DataHelper.CreateParameter("@Email", SqlDbType.NVarChar, 256, obj.Email),
                DataHelper.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, 128, obj.PhoneNumber, true),
                DataHelper.CreateParameter("@Password", SqlDbType.NVarChar, -1, obj.Password, true),
                DataHelper.CreateParameter("@IsLogedIn", SqlDbType.Bit, 1, obj.IsLogedIn),
                DataHelper.CreateParameter("@LastLoginDate", SqlDbType.DateTime, 8, obj.LastLoginDate, true),
                DataHelper.CreateParameter("@AccountStatus", SqlDbType.NVarChar, 256, obj.CurrentStatus),
                DataHelper.CreateParameter("@OnlineStatus", SqlDbType.NVarChar, 16, obj.OnlineStatus),
                DataHelper.CreateParameter("@LastLockDate", SqlDbType.DateTime, 8, obj.LastLockDate, true)
            };
            return ExecuteCommand(sqlText, sqlParams);
		}
        
		public int Delete(string varUserId){
			var sqlText = "Delete From dbo.[ApplicationUsers] Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId)
            };
            return ExecuteCommand(sqlText, sqlParams);
		}
        
	}   
}
	
 
