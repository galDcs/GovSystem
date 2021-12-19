using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default2 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string line = string.Empty;

        System.IO.StreamReader file = new System.IO.StreamReader(@"D:\all_updated_dockets.txt");
        string docketId = string.Empty;

        while ((line = file.ReadLine()) != null)
        {
            if (line.Contains("docket_id ="))
            {
                docketId = line.Substring(line.Length - 6, 6).Trim();

                DataSet ds = DAL_SQL.RunSqlDataSet(@"SELECT docket_id, last_update_date, (SELECT login_name FROM CLERKS WHERE last_update_clerk_id = id) as name 
FROM BUNDLES WHERE 
from_date like '%Nov-17%' AND 
to_date like '%Nov-17%' AND 
income_type = 6 AND 
--last_update_date like '%-18%' AND 
docket_id = " + docketId + @" AND 
--cast(last_update_date as smalldatetime) > cast('11-Jan-18' as smalldatetime) AND 
id in (SELECT bundle_id FROM VOUCHERS WHERE status = 1) 

order by docket_id");

				if (isDataSetRowsNotEmpty(ds))
				{
					Response.Write(line);
				}
            }
        }
    }
	
	private bool isDataSetRowsNotEmpty(DataSet iDataSet)
    {//Check if the DataSet has rows in first table.
        bool isNotEmpty = false;

        if (iDataSet != null && iDataSet.Tables != null && iDataSet.Tables.Count > 0
                && iDataSet.Tables[0].Rows != null && iDataSet.Tables[0].Rows.Count > 0)
        {
            isNotEmpty = true;
        }

        return isNotEmpty;
    }

}