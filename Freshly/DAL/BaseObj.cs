using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Data;
using System.Threading;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace Freshly.Identity.DAL {

    internal abstract class BaseObj : IDisposable
    {

        //public static string ConnStr { get; set; }
        protected int ComTimeout = 60;
        protected SqlConnection Conn;

        public int ExecuteCommand(string commandText, List<SqlParameter> parameters, CommandType cmdType = CommandType.Text)
        {
            int result = 0;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }

            try
            {
                if (Conn == null) Conn = new SqlConnection(Freshly.D.ConnectionString);
                using (SqlCommand cmd = new SqlCommand(commandText, Conn))
                {
                    OpenConnection();
                    cmd.Parameters.AddRange(parameters.ToArray());
                    cmd.CommandType = cmdType;
                    result = cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        public object GetValue(string commandText, List<SqlParameter> parameters, CommandType cmdType = CommandType.Text)
        {
            object result = null;

            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentException("Command text cannot be null or empty.");
            }
            try
            {
                if (Conn == null) Conn = new SqlConnection(Freshly.D.ConnectionString);
                using (SqlCommand cmd = new SqlCommand(commandText, Conn))
                {
                    OpenConnection();
                    cmd.Parameters.AddRange(parameters.ToArray());
                    cmd.CommandType = cmdType;
                    result = cmd.ExecuteScalar();
                }
            }
            finally
            {
                CloseConnection();
            }

            return result;
        }

        protected void OpenConnection()
        {
            var retries = 3;
            if (Conn.State == ConnectionState.Open)
            {
                return;
            }
            else
            {
                while (retries >= 0 && Conn.State != ConnectionState.Open)
                {
                    Conn.Open();
                    retries--;
                    Thread.Sleep(30);
                }
            }
        }

        protected void CloseConnection()
        {
            if (Conn.State == ConnectionState.Open)
            {
                Conn.Close();
            }
        }

        // Add methods and properties you want to expose to all the derived classes.
        
        public void Dispose()
        {
            if (Conn != null)
            {
                Conn.Dispose();
                Conn = null;
            }
        }
    }
}

