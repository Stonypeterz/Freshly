using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
			var sqlText = "Insert Into dbo.[ApplicationUsers] ([UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [AccountStatus]) Values (@UserId, @FirstName, @LastName, @DateOfBirth, @Gender, @Email, @PhoneNumber, @Password, @AccountStatus)";
            var sqlParams = new List<SqlParameter>
            {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, obj.UserId),
                DataHelper.CreateParameter("@FirstName", SqlDbType.NVarChar, 256, obj.FirstName),
                DataHelper.CreateParameter("@LastName", SqlDbType.NVarChar, 256, obj.LastName),
                DataHelper.CreateParameter("@DateOfBirth", SqlDbType.DateTime, 8, obj.DateOfBirth, true),
                DataHelper.CreateParameter("@Gender", SqlDbType.NVarChar, 128, obj.Gender),
                DataHelper.CreateParameter("@Email", SqlDbType.NVarChar, 256, obj.Email),
                DataHelper.CreateParameter("@AccountStatus", SqlDbType.NVarChar, 256, obj.CurrentStatus),
                DataHelper.CreateParameter("@PhoneNumber", SqlDbType.NVarChar, 128, obj.PhoneNumber, true),
                DataHelper.CreateParameter("@Password", SqlDbType.NVarChar, -1, obj.Password)
            };
            return ExecuteCommand(sqlText, sqlParams);
		}
        
        public UsersPage GetPage(int PageNo, int PageSize = 10, string filterQ = "0") {
			using (SqlCommand cmd = new SqlCommand("Declare @Total int;if(@filterQ = '0') Begin With PageObject As (Select Row_Number() Over (Order By [FirstName]) As RowNumber, [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [IsLogedIn], [LastLoginDate], [AccountStatus] From dbo.[ApplicationUsers]) Select [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [IsLogedIn], [LastLoginDate], [AccountStatus], [LockWindow], [MaxLoginAttempts] From PageObject Q Where Q.RowNumber <= (@PageNo * @PageSize) And Q.RowNumber > ((@PageNo * @PageSize) - @PageSize);Select @Total = Count(*) From dbo.[ApplicationUsers];if(@Total % @PageSize = 0) Select @Total / @PageSize As 'PageCount' else Select ((@Total - (@Total % @PageSize))/@PageSize) + 1 As 'PageCount' End else Begin With PageObject As (Select Row_Number() Over (Order By [FirstName]) As RowNumber, [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [IsLogedIn], [LastLoginDate], [AccountStatus], [LockWindow], [MaxLoginAttempts] From dbo.[ApplicationUsers] Where [UserId] Like '%' + @filterQ + '%' Or [FirstName] Like '%' + @filterQ + '%' Or [LastName] Like '%' + @filterQ + '%' Or [Gender] Like '%' + @filterQ + '%' Or [Email] Like '%' + @filterQ + '%' Or [PhoneNumber] Like '%' + @filterQ + '%' Or [Password] Like '%' + @filterQ + '%' Or [AccountStatus] Like '%' + @filterQ + '%') Select [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [IsLogedIn], [LastLoginDate], [AccountStatus], [LockWindow], [MaxLoginAttempts] From PageObject Q Where Q.RowNumber <= (@PageNo * @PageSize) And Q.RowNumber > ((@PageNo * @PageSize) - @PageSize);Select @Total = Count(*) From dbo.[ApplicationUsers] Where [UserId] Like '%' + @filterQ + '%' Or [FirstName] Like '%' + @filterQ + '%' Or [LastName] Like '%' + @filterQ + '%' Or [Gender] Like '%' + @filterQ + '%' Or [Email] Like '%' + @filterQ + '%' Or [PhoneNumber] Like '%' + @filterQ + '%' Or [Password] Like '%' + @filterQ + '%' Or [AccountStatus] Like '%' + @filterQ + '%';if(@Total % @PageSize = 0) Select @Total / @PageSize As 'PageCount' else Select ((@Total - (@Total % @PageSize))/@PageSize) + 1 As 'PageCount' End", Conn)) {
				cmd.CommandTimeout = ComTimeout;
				cmd.Parameters.AddWithValue("@PageNo", PageNo);
				cmd.Parameters.AddWithValue("@PageSize", PageSize);
				cmd.Parameters.AddWithValue("@filterQ", string.IsNullOrEmpty(filterQ) == true ? "0" : filterQ);
				List<ApplicationUser> objList = new List<ApplicationUser>();
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
                                Password = (dr["Password"] == System.DBNull.Value) ? null : (string)dr["Password"],
                                IsLogedIn = (bool)dr["IsLogedIn"],
                                LastLoginDate = (dr["LastLoginDate"] == System.DBNull.Value) ? null : (DateTime?)dr["LastLoginDate"],
                                CurrentStatus = (dr["AccountStatus"] == System.DBNull.Value) ? null : (string)dr["AccountStatus"],
                            };

                            objList.Add(obj);
						}
						dr.NextResult();
						while (dr.Read()) {
							TotalPages = (int)dr["PageCount"];
						}
					}
					return new UsersPage(objList, PageNo, PageSize, TotalPages, filterQ);
					}
				finally {
					CloseConnection();
				}
			}

		}

        public void SetLogedIn(bool status, ApplicationUser usr)
        {
            try
            {
                if (usr != null)
                {
                    usr.IsLogedIn = status;
                    usr.LastLoginDate = DateTime.UtcNow;
                    Update(usr);
                }
            }
            catch (Exception ex) { }
        }

        public void SetLogedIn(bool status, string usrName)
        {
            try { SetLogedIn(status, GetUserByID(usrName)); }
            catch (Exception ex) { }
        }

        private ApplicationUser GetRecordByID(string varSql, List<SqlParameter> sqlParams) {
			using (SqlCommand cmd = new SqlCommand($"Select [UserId], [FirstName], [LastName], [DateOfBirth], [Gender], [Email], [PhoneNumber], [Password], [IsLogedIn], [LastLoginDate], [AccountStatus], [LastLockDate], [LastActivityDate], [RegDate] From dbo.[ApplicationUsers] Where {varSql}", Conn)) {
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
                                Password = (dr["Password"] == System.DBNull.Value) ? null : (string)dr["Password"],
                                IsLogedIn = (bool)dr["IsLogedIn"],
                                LastLoginDate = (dr["LastLoginDate"] == System.DBNull.Value) ? null : (DateTime?)dr["LastLoginDate"],
                                CurrentStatus = (dr["AccountStatus"] == System.DBNull.Value) ? null : (string)dr["AccountStatus"],
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
                                if (countr == 0) obj.Groups += dr["GroupName"].ToString();
                                else obj.Groups += $",{dr["GroupName"].ToString()}";
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
            string sql = "([UserId] = @UserId OR [Email] = @Email OR [PhoneNumber] = @PhoneNumber);";
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
            using (SqlCommand cmd = new SqlCommand("Select UserId, FirstName, LastName From dbo.ApplicationUsers Where UserId = @UserId; Select A.GroupName, Case When (Select B.GroupName From dbo.UserGroups B Where A.GroupName = B.GroupName And B.UserId = @UserId) = null Then 'false' Else 'true' End As IsAssigned From dbo.ApplicationGroups A;", Conn)) {
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
			var sqlText = "Update dbo.[ApplicationUsers] Set [FirstName] = @FirstName, [LastName] = @LastName, [DateOfBirth] = @DateOfBirth, [Gender] = @Gender, [Email] = @Email, [PhoneNumber] = @PhoneNumber, [Password] = @Password, [IsLogedIn] = @IsLogedIn, [LastLoginDate] = @LastLoginDate, [AccountStatus] = @AccountStatus, [LastLockDate] = @LastLockDate, [LastActivityDate] = GetDate() Where ([UserId] = @UserId)";
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
                DataHelper.CreateParameter("@AccountStatus", SqlDbType.NVarChar, 256, obj.CurrentStatus, true),
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
	
 
