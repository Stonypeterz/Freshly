using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Freshly.Identity.Models;

namespace Freshly.Identity {

    internal class UserAvatarsFactory : BaseObj {
        
		public UserAvatarsFactory() {
			Conn = new SqlConnection(Freshly.D.ConnectionString);
		}

        public int Insert(string varUserId, byte[] varAvatar, string varExtension, string varMimeType, DateTime varDateUpdated)
        {
            var sqlText = "Insert Into dbo.[UserAvatars] ([UserId], [Avatar], [Extension], [MimeType], [DateUpdated]) Values (@UserId, @Avatar, @Extension, @MimeType, @DateUpdated)";
            var sqlParams = new List<SqlParameter>() {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId),
                DataHelper.CreateParameter("@Avatar", SqlDbType.VarBinary, -1, varAvatar),
                DataHelper.CreateParameter("@Extension", SqlDbType.VarChar, 10, varExtension),
                DataHelper.CreateParameter("@MimeType", SqlDbType.VarChar, 128, varMimeType),
                DataHelper.CreateParameter("@DateUpdated", SqlDbType.DateTime, 8, varDateUpdated),
            };
            return ExecuteCommand(sqlText, sqlParams);
        }

        public int Insert(UserAvatar obj)
        {
            return Insert(obj.UserId, obj.Avatar, obj.Extension, obj.MimeType, obj.DateUpdated);
        }

        public UserAvatar GetAvatar(string varUserId)
        {
            using (SqlCommand cmd = new SqlCommand("Select A.[UserId], [Gender], [Avatar], [Extension], [MimeType], [DateUpdated] From dbo.[UserAvatars] A Inner Join dbo.[ApplicationUsers] B On A.[UserId] = B.[UserId] Where (A.[UserId] = @UserId);Select [UserId], [Gender] From dbo.[ApplicationUsers] Where ([UserId] = @UserId);", Conn)) {
                cmd.CommandTimeout = ComTimeout;
                cmd.Parameters.Add(DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId));

                UserAvatar obj = null;
                try {
                    OpenConnection();
                    using (SqlDataReader dr = cmd.ExecuteReader()) {
                        while (dr.Read()) {
                            obj = new UserAvatar() {
                                UserId = (string)dr["UserId"],
                                Gender = (string)dr["Gender"],
                                Avatar = (byte[])dr["Avatar"],
                                Extension = (string)dr["Extension"],
                                MimeType = (string)dr["MimeType"],
                                DateUpdated = (DateTime)dr["DateUpdated"],
                                IsDefault = false 
                            };
                        }
                        if(obj == null) {
                            dr.NextResult();
                            while (dr.Read()) {
                                obj = new UserAvatar() {
                                    UserId = (string)dr["UserId"],
                                    Gender = (string)dr["Gender"],
                                    Avatar = (string)dr["Gender"] == "Male" ? Properties.Resources.MaleAvatar : Properties.Resources.FemaleAvatar,
                                    Extension = "png",
                                    MimeType = "image/png",
                                    DateUpdated = DateTime.UtcNow,
                                    IsDefault = true
                                };
                            }
                        }
                    }
                    return obj;
                } finally {
                    CloseConnection();
                }
            }

        }

        public int Update(string varUserId, byte[] varAvatar, string varExtension, string varMimeType, DateTime varDateUpdated)
        {
            var sqlText = "Update dbo.[UserAvatars] Set [Avatar] = @Avatar, [Extension] = @Extension, [MimeType] = @MimeType, [DateUpdated] = @DateUpdated Where ([UserId] = @UserId)";
            var sqlParams = new List<SqlParameter>() {
                DataHelper.CreateParameter("@UserId", SqlDbType.NVarChar, 128, varUserId),
                DataHelper.CreateParameter("@Avatar", SqlDbType.VarBinary, -1, varAvatar),
                DataHelper.CreateParameter("@Extension", SqlDbType.VarChar, 10, varExtension),
                DataHelper.CreateParameter("@MimeType", SqlDbType.VarChar, 128, varMimeType),
                DataHelper.CreateParameter("@DateUpdated", SqlDbType.DateTime, 8, varDateUpdated),
            };
            return ExecuteCommand(sqlText, sqlParams);
        }

        public int Update(UserAvatar obj)
        {
            return Update(obj.UserId, obj.Avatar, obj.Extension, obj.MimeType, obj.DateUpdated);
        }
        
    }

}
