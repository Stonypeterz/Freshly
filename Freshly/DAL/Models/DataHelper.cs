using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

public class DataHelper 
{
    internal static SqlParameter CreateParameter(string paramName, SqlDbType dbType, int Size, object Value, bool IsNullable = false)
    {
        var sqlParam = new SqlParameter(paramName, dbType, Size)
        {
            IsNullable = IsNullable,
            Value = Value ?? DBNull.Value
        };
        return sqlParam;
    }
}

