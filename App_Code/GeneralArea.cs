using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;

/// <summary>
/// Summary description for GovGeneralArea
/// </summary>
public class GeneralArea
{
    public int GeneralAreaId { get; set; }
    public string GeneralAreaName { get; set; }

	public GeneralArea()
	{

	}

    public static List<GeneralArea> LoadGeneralAreas(GovTraveller traveller)
    {
        string makat1, makat2;
        makat1 = traveller.SelectedMakat[0].ItemSKU;
        
        // currently assume that no more than 2 makats (it's bu design)
        if (traveller.SelectedMakat.Count > 1)
            makat2 = traveller.SelectedMakat[1].ItemSKU;
        else
            makat2 = "";

        return parseResult(DAL_SQL_Helper.GetGeneralAreas(makat1, makat2));
    }

    private static List<GeneralArea> parseResult(DataSet dataSet)
    {
        List<GeneralArea> list = new List<GeneralArea>();

        foreach (DataRow dr in dataSet.Tables[0].Rows)
        {
            list.Add(new GeneralArea()
            {
                GeneralAreaId = int.Parse(dr["general_area_id"].ToString()),
                GeneralAreaName = dr["general_area_name"].ToString()
            });
        }
        return list;
    }

}
