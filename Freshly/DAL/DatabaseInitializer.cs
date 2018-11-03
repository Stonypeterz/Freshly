using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Freshly.Identity
{
    internal class DatabaseInitializer : BaseObj
    {

        public string SqlQuery { get; set; }
        public string DBName { get; set; }

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
            var sql = string.Format(SqlQuery, DBName);
            try
            {
                using (var tConn = new SqlConnection(Freshly.D.ConnectionString.Replace(DBName, "master")))
                {
                    using (var cmd = new SqlCommand(sql, tConn))
                    {
                        tConn.Open();
                        cmd.ExecuteNonQuery();
                        tConn.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                f = false;
            }
            return f;
        }
    }
}
