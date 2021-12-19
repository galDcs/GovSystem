using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Xml;

public class DAL_SQL
{
    private static string connStr = String.Empty;
    public static string ConnStr
    {
        get
        {
            if (connStr == String.Empty)
            {
                //aviran 24/08 - first line remove comment second delete - 
                //connStr = ConfigurationManager.AppSettings.Get("connectionString");
                connStr = ConfigurationManager.AppSettings.Get("CurrentAgencyDBConnStr");
            }
						
			//Check if illegal DB.
            if (connStr.Contains("AGN_OUT_00"))
            {
                string agnUserId, agnDbType;

                agnUserId = ConfigurationManager.AppSettings.Get("AgencyUserId");
                agnDbType = ConfigurationManager.AppSettings.Get("AgencyDbType");

                connStr = connStr.Replace("OUT", agnDbType);
                connStr = connStr.Replace("00", "00" + agnUserId);
            }
			
            return connStr;
        }
        set
        {
            if (value != null && value != "")
            {
                connStr = value;
            }
        }
    }

    public static DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        SqlCommand command = new SqlCommand();
        ConnStr = connectionString;
        DataSet ds = new DataSet();

        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;
			command.CommandTimeout = 250;
            if (commandParameters != null || command.CommandType == CommandType.StoredProcedure)
            {
                AttachParameters(command, commandParameters);
            }

            //create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(command);
            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.                                            
            command.Parameters.Clear();
        }

        //return the dataset
        return ds;
    }

    private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
    {
        if (command.CommandType != CommandType.StoredProcedure ||
            (command.CommandType == CommandType.StoredProcedure && commandParameters != null))
        {
            foreach (SqlParameter p in commandParameters)
            {
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }
    }

    public static string ExecuteXmlString(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        SqlDataReader dr = null;
        SqlCommand command = new SqlCommand();


        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (commandParameters != null || command.CommandType == CommandType.StoredProcedure)
            {
                AttachParameters(command, commandParameters);
            }

            try
            {
                dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                return GetXmlExecXmlString(dr, "root");
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                throw e;
            }
            finally
            {
                if (dr != null) dr.Close();
                command.Parameters.Clear();
            }
        }
    }

    public static string ExecuteXmlString(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
    {
        SqlDataReader dr = null;
        SqlCommand command = new SqlCommand();
        ConnStr = connectionString;

        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = commandType;

            if (commandParameters != null || command.CommandType == CommandType.StoredProcedure)
            {
                AttachParameters(command, commandParameters);
            }

            try
            {
                dr = command.ExecuteReader(CommandBehavior.CloseConnection);
                return GetXmlExecXmlString(dr, "root");
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
                throw e;
            }
            finally
            {
                if (dr != null) dr.Close();
                command.Parameters.Clear();
            }
        }
    }



    /// <summary>
    /// returns string in xml format as fetched from db and wrapped in parent tags if given
    /// </summary>
    /// <remarks>
    ///  xml can be fetched by using several selects FOR XML...
    /// </remarks>
    /// <param name="dr">filled SqlDataReader</param>
    /// <param name="parentTag">either legal xml tag name or ""</param>
    /// <returns>string in xml format (not necessarily well formated)</returns>
    private static string GetXmlExecXmlString(SqlDataReader dr, string parentTag)
    {
        string retval = String.Empty;
        StringBuilder sb = new StringBuilder();
        do
        {
            if (dr.Read())
            {
                if (!dr.IsDBNull(0))
                {
                    sb.Append(dr.GetString(0));
                }
                while (dr.Read())
                {
                    if (!dr.IsDBNull(0))
                    {
                        sb.Append(dr.GetString(0));
                    }
                }
            }
        }
        while (dr.NextResult());

        if (parentTag != null && parentTag != "")
        {
            sb.Insert(0, "<" + parentTag + ">");
            sb.Append("</" + parentTag + ">");
        }

        return sb.ToString();
    }

    public static string GetRecord(string TableName, string SelectField, string Key, string SearchField)
    {
        if (TableName.Length <= 0 || SelectField.Length <= 0 || Key.Length <= 0 || SearchField.Length <= 0)
            return "";

        string commandText = " select " + SelectField + " from " + TableName + " where " + SearchField + " = " + Key + " ";

        SqlCommand command = new SqlCommand();
        //ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"]; //connectionString;
        DataSet ds = new DataSet();

        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;

            //create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(command);

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.                                            
            command.Parameters.Clear();
        }

        try // to return value
        {
            return ds.Tables[0].Rows[0].ItemArray[0].ToString();
        }
        catch (Exception ex) // no value
        {
			if (!ex.Message.Contains("There is no row at position 0."))
			{
				Logger.Log(ex.Message);
			}
            return "";
        }
    }

    public static string RunSql(string sql)
    {
        if (sql.Length <= 0)
            return "";

        string commandText = sql;

        SqlCommand command = new SqlCommand();
        //ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"]; //connectionString;
        DataSet ds = new DataSet();

        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;

            //create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(command);

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.                                            
            command.Parameters.Clear();
        }

        try // to return value
        {
            return ds.Tables[0].Rows[0].ItemArray[0].ToString();
        }
        catch (Exception ex) // no value
        {
            Logger.Log(ex.Message);
            return "";
        }
    }
    public static bool RunSqlbool(string sql)
    {
        if (sql.Length <= 0)
            return false;

        string commandText = sql;

        SqlCommand command = new SqlCommand();

        // ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"]; //connectionString;
        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;

            //create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(command);
            DataSet ds = new DataSet();

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.                                            
            command.Parameters.Clear();
        }

        try // to return value
        {
            return true;
        }
        catch (Exception ex) // no value
        {
            Logger.Log(ex.Message);
            return false;
        }
    }
	
	   public static DataSet RunSqlDataSet(string sql)
    {
        if (sql.Length <= 0)
            return null;

        string commandText = sql;

        SqlCommand command = new SqlCommand();
        //ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"]; //connectionString;
        DataSet ds = new DataSet();

        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;

            //create the DataAdapter & DataSet
            SqlDataAdapter da = new SqlDataAdapter(command);

            //fill the DataSet using default values for DataTable names, etc.
            da.Fill(ds);

            // detach the SqlParameters from the command object, so they can be used again.                                            
            command.Parameters.Clear();
        }

        return ds;
    }
	
    public static bool UpdateRecord(string TableName, string UpdateField, string UpdateValue, string Key, string SearchField)
    {
        if (TableName.Length <= 0 || UpdateField.Length <= 0 || UpdateValue.Length <= 0 || Key.Length <= 0 || SearchField.Length <= 0)
            return false;

        string commandText = " update " + TableName + " set " + UpdateField + " = '" + UpdateValue + "' where " + SearchField + " = " + Key + " ";

        SqlCommand command = new SqlCommand();
        //ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"]; //connectionString;

        using (var connection = new SqlConnection(ConnStr))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            command.Connection = connection;
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;

            try
            {
                //create the DataAdapter & DataSet
                SqlDataAdapter da = new SqlDataAdapter(command);
                DataSet ds = new DataSet();

                //fill the DataSet using default values for DataTable names, etc.
                da.Fill(ds);

                // detach the SqlParameters from the command object, so they can be used again.                                            
                command.Parameters.Clear();
                return true;
            }
            catch (Exception ex) // no value
            {
                Logger.Log(ex.Message);
                return false;
            }

        }
    }

    //chen - this function retrieve xml 

}

public sealed class SqlDalParam
{
    private SqlDalParam() { }

    public static SqlParameter formatParam(string pname, SqlDbType ptype, object pvalue)
    {
        SqlParameter sparam = new SqlParameter(pname, ptype);
        sparam.Value = pvalue;
        sparam.Direction = ParameterDirection.Input;
        return sparam;
    }

    public static SqlParameter formatParam(string pname, SqlDbType ptype, int psize, object pvalue)
    {
        return formatParam(pname, ptype, psize, pvalue, ParameterDirection.Input);
    }

    public static SqlParameter formatParam(string pname, SqlDbType ptype, int psize, object pvalue, ParameterDirection pdir)
    {
        SqlParameter sparam = new SqlParameter(pname, ptype, psize);
        sparam.Value = pvalue;
        sparam.Direction = pdir;
        return sparam;
    }
}
