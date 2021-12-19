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
using System.Xml;

/// <summary>
/// Summary description for GovTravellerMakat
/// </summary>
public class GovTravellerMakat
{
    public string ItemSKU { get; set; }
    public string ItemDescription { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int DaysNum { get; set; }
    public int EscortNum { get; set; }
    public string Department { get; set; }
    public string Level { get; set; }
    public int UsageBalance { get; set; }
    public int JerusalemUsageBalance { get; set; }

    public int MinNights { get; set; }
    public int MaxNights { get; set; }
    public bool OneTimeUssage { get; set; }
    public bool MakatTipulim { get; set; }
    public bool Allow5And5Nights { get; set; }
    public int StartOrderDateFromTodayMin { get; set; }
    public int StartOrderDateFromTodayMax { get; set; }
    public bool AllowedToAdd5NightForPay { get; set; }
    public string MakatDescription { get; set; }
    public string OfficeRemarkForOrder { get; set; }
    public string VoucherRemark { get; set; }
    public string WriteToAccLogFirstRoom { get; set; }
    public string WriteToAccLogSecondRoom { get; set; }
    public string WriteToAccLogTipulim { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
    public int Makat40 { get; set; }

    public bool DoubledMakat;

    public string Request_SH { get; set; }


    public string OfficeComment { get; set; }
    public GovTravellerMakat()
    {
    }

    public static List<GovTravellerMakat> GetTravellerMaketst(XmlNode travNode)
    {
        List<GovTravellerMakat> makats = new List<GovTravellerMakat>();
        XmlNodeList makatList = travNode.SelectNodes("makat");

        foreach (XmlNode xmlMakat in makatList)
        {
            GovTravellerMakat makat = new GovTravellerMakat();
			if (xmlMakat.Attributes["Status"].Value.ToString() == "1")
            {
            //makats.Add(new GovTravellerMakat()
            //{
            makat.ItemSKU = xmlMakat.Attributes["ItemSKU"].Value;
            makat.ItemDescription = xmlMakat.Attributes["ItemDescription"].Value;
            makat.StartDate = DateTime.Parse(xmlMakat.Attributes["StartDate"].Value);
            makat.EndDate = DateTime.Parse(xmlMakat.Attributes["EndDate"].Value);
            makat.DaysNum = int.Parse(xmlMakat.Attributes["DaysNum"].Value);
            makat.EscortNum = int.Parse(xmlMakat.Attributes["EscortNum"].Value);
            makat.Department = xmlMakat.Attributes["Department"].Value;
            makat.Level = xmlMakat.Attributes["Level"].Value;
            makat.UsageBalance = int.Parse(xmlMakat.Attributes["UsageBalance"].Value);
            makat.MinNights = (xmlMakat.Attributes["MinNights"] != null) ? int.Parse(xmlMakat.Attributes["MinNights"].Value) : 0;
            makat.MaxNights = (xmlMakat.Attributes["MaxNights"] != null) ? int.Parse(xmlMakat.Attributes["MaxNights"].Value) : 0;
            makat.OneTimeUssage = (xmlMakat.Attributes["OneTimeUssage"].Value == "1") ? true : false;
            makat.MakatTipulim = (xmlMakat.Attributes["MakatTipulim"].Value == "1") ? true : false;
            makat.Allow5And5Nights = (xmlMakat.Attributes["Allow5And5Nights"].Value == "1") ? true : false;
            makat.StartOrderDateFromTodayMin = (xmlMakat.Attributes["StartOrderDateFromTodayMin"] != null) ? int.Parse(xmlMakat.Attributes["StartOrderDateFromTodayMin"].Value) : 0;
            makat.StartOrderDateFromTodayMax = (xmlMakat.Attributes["StartOrderDateFromTodayMax"] != null) ? int.Parse(xmlMakat.Attributes["StartOrderDateFromTodayMax"].Value) : 0;
            makat.AllowedToAdd5NightForPay = (xmlMakat.Attributes["AllowedToAdd5NightForPay"].Value == "1") ? true : false;
            makat.MakatDescription = xmlMakat.Attributes["MakatDescription"].Value;
            makat.OfficeRemarkForOrder = xmlMakat.Attributes["OfficeRemarkForOrder"].Value;
            makat.VoucherRemark = xmlMakat.Attributes["VoucherRemark"].Value;
            makat.WriteToAccLogFirstRoom = xmlMakat.Attributes["WriteToAccLogFirstRoom"].Value;
            makat.WriteToAccLogSecondRoom = xmlMakat.Attributes["WriteToAccLogSecondRoom"].Value;
            makat.WriteToAccLogTipulim = xmlMakat.Attributes["WriteToAccLogTipulim"].Value;
            makat.JerusalemUsageBalance = int.Parse(xmlMakat.Attributes["JerusalemUsageBalance"].Value);
            makat.OfficeComment = xmlMakat.Attributes["OfficeComment"].Value;
            makat.ReleaseDate = DateTime.Parse(xmlMakat.Attributes["ReleaseDate"].Value);
            makat.IsActive = (xmlMakat.Attributes["IsActive"].Value == "1") ? true : false;
            makat.Makat40 = int.Parse(xmlMakat.Attributes["Makat40"].Value);
            makat.Request_SH = xmlMakat.Attributes["Request_SH"].Value;
            //});
            makats.Add(makat);
			}
        }
        return makats;
    }
}
