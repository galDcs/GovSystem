using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Xml;

/// <summary>
/// Summary description for GovTraveller
/// </summary>
public class GovTraveller
{
    public string DocketId { get; set; }
    public string TravellerId { get; set; }
    public string FirstName { get; set; }
    public string SecondName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
	public string EntitledYear { get; set; } //@EY
    // for order
    public int RoomsAmount { get; set; }
    public int BaseId { get; set; }
    public int RoomId { get; set; }
    public string AgencyDocketId { get; set; }
    public bool ZakayPays { get; set; }
    public bool MelavePays { get; set; }
    public bool BalanceUssage { get; set; }
    // total order amount
    public double TotalAmountNetto { get; set; }
    public double TotalAmountBruto { get; set; }
    // total order amount
    public double TravellerPayAmount { get; set; }
    public double SibsudAmount { get; set; }

    public bool IsAdded5thNight { get; set; }

    public string Phone1Prefix { get; set; }
    public string Phone1Number { get; set; }

    public string Phone2Prefix { get; set; }
    public string Phone2Number { get; set; }

    public string Phone3Prefix { get; set; }
    public string Phone3Number { get; set; }

    public string StrMelaveSelectedNights { get; set; }

    public string Simon_100 { get; set; }

    public string Nagish { get; set; }

	public DateTime LastDateToOrder { get; set; }
	
    public string Mahadora { get; set; }
    private int balance;
    public int Balance
    {
        get
        {
            //int bal = 0;
            //foreach (GovTravellerMakat makat in this.SelectedMakat)
            //{
            //    //if (bal < makat.UsageBalance) // klal makel better for traveller
            //    //    bal = makat.UsageBalance;
            //    bal = makat.DaysNum - makat.UsageBalance;
            //}
            //return bal;

            return balance;
        }
        set
        {
            balance = value;
        }
    }

    private bool isBalanceSet { set; get; }

    Dictionary<string, Dictionary<string, int>> mDictMakatAndRequestIdCount = null;

    public int JerusalemBalance
    {
        get
        {
            int bal = 0;
            foreach (GovTravellerMakat makat in this.SelectedMakat)
            {
                //if (bal < makat.JerusalemUsageBalance) // klal makel better for traveller
                //    bal = makat.JerusalemUsageBalance;
                bal = makat.DaysNum - makat.JerusalemUsageBalance;
            }
            return bal;
        }
    }


    public List<GovTravellerMakat> Makats = new List<GovTravellerMakat>();
    public List<GovTravellerMelave> Melave = new List<GovTravellerMelave>();
    public List<GovTravellerMakat> SelectedMakat = new List<GovTravellerMakat>();
    public string ErkevType = string.Empty;
    public string FourOneSeven = string.Empty;
    public bool InternrtDocket = false;

    private static string UserSessionVariableName = "UserSessionVariableName";



    private bool zakautKfula1 = false;
    public bool ZakautKfula1
    {
        get { return zakautKfula1; }
    }

    private bool zakautKfula2 = false;
    public bool ZakautKfula2
    {
        get { return zakautKfula2; }
    }

    public bool HasMoreThanOneMAKAT
    {
        get { return (Makats.Count > 0); }
    }

    public bool HasMakatKaful
    {
        get { return (true); }
    }

    public GovTraveller()
    {
        isBalanceSet = false;
    }

    public GovTraveller LoadDataByTravellerId(string travellerId)
    {
        return parseSearchResult(DAL_SQL_Helper.GetTravellerDetails(travellerId, string.Empty));
    }

    public GovTraveller LoadDataByDocketId(string docketId)
    {
        GovTraveller travellerByDocket = parseSearchResult(DAL_SQL_Helper.GetTravellerDetails(string.Empty, docketId));

        return travellerByDocket;
    }

    public static XmlDocument LoadOrdersHistoryDataByTravellerId(string travellerId)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(DAL_SQL_Helper.GetTravellerOrdersHistoryDetails(travellerId, "0"));
        return xmlDoc;
    }

    public static XmlDocument LoadOrdersHistoryDataByDocketId(string docketId)
    {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(DAL_SQL_Helper.GetTravellerOrdersHistoryDetails("0", docketId));
        return xmlDoc;
    }

    public string GetContacts()
    {
        string mobile = "";
        string workPhone = "";
        string homePhone = "";
        string fax = "";

        string result = "";

        if (Phone1Prefix.IndexOf("05") == 0)
        {
            mobile = Phone1Prefix + Phone1Number;
            workPhone = Phone2Prefix + Phone2Number;
            homePhone = Phone3Prefix + Phone3Number;
        }

        if (Phone2Prefix.IndexOf("05") == 0)
        {
            mobile = Phone2Prefix + Phone2Number;
            workPhone = Phone1Prefix + Phone1Number;
            homePhone = Phone3Prefix + Phone3Number;
        }

        if (Phone3Prefix.IndexOf("05") == 0)
        {
            mobile = Phone3Prefix + Phone3Number;
            workPhone = Phone1Prefix + Phone1Number;
            homePhone = Phone2Prefix + Phone2Number;
        }

        result += "<MOBILE>" + mobile.Replace(" ", "") + "</MOBILE>";
        result += "<WORK>" + workPhone.Replace(" ", "") + "</WORK>";
        result += "<HOME>" + homePhone.Replace(" ", "") + "</HOME>";
        result += "<FAX>" + fax.Replace(" ", "") + "</FAX>";
        return result;
    }

    private GovTraveller parseSearchResult(string travellerResult)
    {
        XmlDocument xmlDoc = new XmlDocument();
		
        xmlDoc.LoadXml(travellerResult);

        XmlNodeList travNodes = xmlDoc.FirstChild.SelectNodes("traveller");
        foreach (XmlNode travNode in travNodes)
        {
            this.DocketId = travNode.Attributes["DocketId"].Value;
            this.TravellerId = travNode.Attributes["TravellerId"].Value;
            this.FirstName = travNode.Attributes["FirstName"].Value.Replace("'", "`");
            this.SecondName = travNode.Attributes["SecondName"].Value.Replace("'", "`");
            this.Address = travNode.Attributes["Address"].Value.Replace("'", "`");
            this.City = travNode.Attributes["City"].Value.Replace("'", "`");
            this.ZipCode = travNode.Attributes["ZipCode"].Value;
            this.Phone1Prefix = travNode.Attributes["Tel_Pr_1"].Value;
            this.Phone1Number = travNode.Attributes["Tel_Num_1"].Value;
            this.Phone2Prefix = travNode.Attributes["Tel_Pr_2"].Value;
            this.Phone2Number = travNode.Attributes["Tel_Num_2"].Value;
            this.Phone3Prefix = travNode.Attributes["Tel_Pr_3"].Value;
            this.Phone3Number = travNode.Attributes["Tel_Num_3"].Value;
            this.Simon_100 = travNode.Attributes["Simon_100"].Value;
            this.Nagish = travNode.Attributes["Nagish"].Value;
            this.Mahadora = travNode.Attributes["Mahadora"].Value;
			
            this.Makats = GovTravellerMakat.GetTravellerMaketst(travNode);
        }

        //if (this.Makats.Any(m => m.ItemSKU == "027242") && this.Makats.Any(m => m.ItemSKU == "027241"))
        //{
        //    zakautKfula1 = true;

        //    var list = (from m in Makats
        //                where m.ItemSKU == "027242" || m.ItemSKU == "027241"
        //                select m).ToList();

        //    foreach (GovTravellerMakat m in list)
        //    {
        //        m.DoubledMakat = true;
        //    }
        //}
        //else if (this.Makats.Any(m => m.ItemSKU == "027242") && (this.Makats.Any(m => m.ItemSKU == "027236") || this.Makats.Any(m => m.ItemSKU == "027235")) )
        //{
        //    zakautKfula2 = true;

        //    var list = (from m in Makats
        //                where m.ItemSKU == "027242" || m.ItemSKU == "027236" || m.ItemSKU == "027235"
        //                select m).ToList();

        //    foreach (GovTravellerMakat m in list)
        //    {
        //        m.DoubledMakat = true;
        //    }
        //}
        this.Makats = this.Makats.OrderByDescending(t => t.DoubledMakat).ToList();
        return this;
    }

    // save trav traveller session
    public void SaveToSession()
    {
        HttpContext.Current.Session["Traveller"] = this;
    }

    // retrieve traveller from session
    public static GovTraveller LoadFromSession()
    {
        if (HttpContext.Current.Session["Traveller"] == null)
            throw new Exception("שגיאה , יש לרענן את הדף.");
        else if (HttpContext.Current.Session["Traveller"] is GovTraveller)
            return (GovTraveller)HttpContext.Current.Session["Traveller"];
        else
            throw new Exception("שגיאה, יש להכנס למערכת מחדש.");
    }

    // OnBaseStr=0|2|0|0
    public string getBaseStr()
    {
        // +1 for traveller itself
        return this.BaseId.ToString() + "|" + (this.Melave.Count + 1).ToString() + "|0|0,";
    }

    // RoomTypeStr=2|1
    public string getRoomStr()
    {
        string strRet = string.Empty;

        if (this.Melave.Count > 1) // zakai + 2 melave
        {
            this.RoomId = 1; // SGL
            this.RoomsAmount = 2;
            this.Melave[0].RoomId = 2; //DBL
            strRet = this.RoomId + "|1," + this.Melave[0].RoomId + "|1,";
        }
        else if (this.Melave.Count == 1) // zakai + melave
        {
            this.RoomId = 2; // DBL
            strRet = this.RoomId + "|1,";
        }
        else // zakai only
        {
            this.RoomId = 1; // SGL
            strRet = this.RoomId + "|1,";
        }
        this.SaveToSession();
        return strRet;
    }

    public bool makatSelected(string makatToCheck)
    {
        foreach (GovTravellerMakat makat in this.SelectedMakat)
        {
            if (makat.ItemSKU == makatToCheck) return true;
        }
        return false;
    }

    public GovTravellerMakat getSelectedMakatByNumber(string makatNumber)
    {
        foreach (GovTravellerMakat makat in this.SelectedMakat)
        {
            if (makat.ItemSKU == makatNumber) return makat;
        }
        return null;
    }

    public int getOrderMinNights()
    {
        int min = 0;
        // getting min nights from selected makats
        foreach (GovTravellerMakat makat in SelectedMakat)
        {
            // check if no limits
            if (makat.MinNights == 0) return 0;
            min = makat.MinNights;
        }
        return min;
    }

    public int getOrderMaxNights()
    {
        int max = 0;
        // getting max nights from selected makats
        foreach (GovTravellerMakat makat in SelectedMakat)
        {
            // check if no limits
            if (makat.MaxNights == 0) return 0;
            if (makat.MaxNights > max)
                max = makat.MaxNights;
        }
        return max;
    }

    public int getMinStartOrderDate()
    {
        int min = 0;
        // getting start order date min
        foreach (GovTravellerMakat makat in SelectedMakat)
        {
            if (makat.StartOrderDateFromTodayMin == 0) return 0;
            min = makat.StartOrderDateFromTodayMin;
        }
        return min;
    }

    public int getMaxStartOrderDate()
    {
        int max = 0;
        // getting start order date max
        foreach (GovTravellerMakat makat in SelectedMakat)
        {
            if (makat.StartOrderDateFromTodayMax == 0) return 0;
            if (max < makat.StartOrderDateFromTodayMax)
                max = makat.StartOrderDateFromTodayMax;
        }
        return max;
    }

    public bool IsAllovedToAdd5thNight()
    {
        foreach (GovTravellerMakat makat in SelectedMakat)
        {
            if (makat.AllowedToAdd5NightForPay == true) return true;
        }
        return false;
    }


    // chen. set traveller balance per makar.

    public void setTravellerBalance(Dictionary<string, Dictionary<string, int>> iDictMakatAndRequestIdCount)
    {
        foreach (GovTravellerMakat makat in Makats)
        {
            if (iDictMakatAndRequestIdCount.ContainsKey(makat.ItemSKU))
            {
                if (iDictMakatAndRequestIdCount[makat.ItemSKU].ContainsKey(makat.Request_SH))
                {
                    makat.UsageBalance = iDictMakatAndRequestIdCount[makat.ItemSKU][makat.Request_SH];
                }
                else
                {
                    makat.UsageBalance = 0;
                }
            }
            else
            {
                makat.UsageBalance = 0;
            }
        }
    }
	
	public void setMakatAstmaAndTipulimMaxDaysNumber(int iMaxDaysToShow)
    {
        bool hasAstma = false;
        bool hasTipulim = false;

        foreach (GovTravellerMakat makat in Makats)
        {
            if (makat.ItemSKU == "027242")
                hasAstma = true;
            if (makat.ItemSKU == "027236" || makat.ItemSKU == "027235" || makat.ItemSKU == "027241")
                hasTipulim = true;

        }

        if (hasAstma && hasTipulim)
        {
            foreach (GovTravellerMakat makat in Makats)
            {
                if (makat.ItemSKU == "027242" || makat.ItemSKU == "027236" || makat.ItemSKU == "027235" || makat.ItemSKU == "027241")
                {
                    //chen - setting the max days of the biggest value between the unique katalog numbers(itemSKU) to all katalog numbers tipulim 
                    makat.DaysNum = iMaxDaysToShow;
                    //chen - setting the max days of the lowest value between the unique katalog numbers(itemSKU) to all katalog numbers tipulim 
                    makat.MinNights = 14;
                }
            }
        }
    }
}
