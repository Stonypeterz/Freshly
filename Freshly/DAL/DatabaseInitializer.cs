using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Freshly.Identity
{
    internal class DatabaseInitializer : BaseObj
    {

        private string SqlQuery { get; set; }
        private string DBName { get; set; }

        public DatabaseInitializer()
        {
            Conn = new SqlConnection(Freshly.D.ConnectionString);
            DBName = Conn.Database;
            Conn.ConnectionString = Freshly.D.ConnectionString.Replace(DBName, "master");
            SqlQuery = Properties.Resources.DatabaseSchema;
        }

        public bool SetupDB()
        {
            bool f = true;
            var sql = $"DECLARE @dbname nvarchar(128); SET @dbname = N'{DBName}'; IF NOT (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = @dbname OR name = @dbname))) CREATE DATABASE {DBName};";
            try
            {
                using (var cmd = new SqlCommand(sql, Conn))
                {
                    //Create database
                    OpenConnection();
                    cmd.ExecuteNonQuery();
                    OpenConnection();
                }
                //Create all the tables
                var tbls = Regex.Split(SqlQuery, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);//SqlQuery.Split("GO");
                Conn = new SqlConnection(Freshly.D.ConnectionString);
                using (var cmd = new SqlCommand(sql, Conn))
                {
                    try
                    {
                        OpenConnection();
                        foreach (string tb in tbls)
                        {
                            cmd.CommandText = tb;
                            cmd.ExecuteNonQuery();
                        }
                        CloseConnection();
                    }
                    catch(Exception ex)
                    {
                        f = false;
                        CloseConnection();
                    }
                }
            }
            catch(Exception ex)
            {
                f = false;
                CloseConnection();
            }
            return f;
        }
    }
}
