using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.IO;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Xml;


/// <summary>
/// Summary description for Utils
/// </summary>
public static class Utils
{
    
    public static string ConvertDateTimeToHeb(DayOfWeek dayOfWeek)
    {
        switch (dayOfWeek)
        {
            case DayOfWeek.Sunday:
                return "א";
            case DayOfWeek.Monday:
                return "ב";
            case DayOfWeek.Tuesday:
                return "ג";
            case DayOfWeek.Wednesday:
                return "ד";
            case DayOfWeek.Thursday:
                return "ה";
            case DayOfWeek.Friday:
                return "ו";
            case DayOfWeek.Saturday:
                return "ש";
        }
        return " ";
    }
	
	
        public static void setWeekendAndBussinessDays(DateTime iFromDate, DateTime iToDate, out int iBussinessDays, out int iWeekendDays)
        {
            iBussinessDays = getBussinessDays(iFromDate, iToDate);
            iWeekendDays = getWeekendDays(iFromDate, iToDate);
        }

        public static int getBussinessDays(DateTime iFromDate, DateTime iToDate)
        {
            int bussinessDays = 0;
            int i = 0;
            DateTime currDay = iFromDate;

            while (currDay != iToDate)
            {
                switch (currDay.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                        bussinessDays++;
                        break;
                }

                i++;
                currDay = iFromDate.AddDays(i);
            }

            return bussinessDays;
        }

        public static int getWeekendDays(DateTime iFromDate, DateTime iToDate)
        {
            int weekendDays = 0;
            int i = 0;
            DateTime currDay = iFromDate;

            while (currDay != iToDate)
            {
                switch (currDay.DayOfWeek)
                {
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                    case DayOfWeek.Saturday:
                        weekendDays++;
                        break;
                }

                i++;
                currDay = iFromDate.AddDays(i);
            }

            return weekendDays;
        }
		
		public static bool IsWeekend(DateTime iDate)
		{
			bool isWeekend = false;
			
			switch (iDate.DayOfWeek)
			{
				case DayOfWeek.Thursday:
				case DayOfWeek.Friday:
				case DayOfWeek.Saturday:
					isWeekend = true;
					break;
			}
			
			return isWeekend;
		}


    public static string ConvertStringToAgencyUtf(string str)
    {
        string tmp = "";
        if (str != null && str != string.Empty)
        {
            foreach (char c in str)
            {
                tmp = tmp + "&#" + Convert.ToString((int)(c)) + ";";
            }
            return HttpUtility.UrlEncode(tmp);
        }
        return string.Empty;
    }

    public static string AddAbsentSignsToTheBeginningOfTheString(string value, int expectedValueLenght, string sign)
    {
        return AddAbsentSignsToTheBeginningOfTheString(value, expectedValueLenght, sign, CutFrom.Start, AddTo.Start);
    }
    
    public static string AddAbsentSignsToTheBeginningOfTheString(string value, int expectedValueLenght, string sign, CutFrom cutFrom, AddTo addTo)
    {
        if (value.Length == expectedValueLenght) {
            return value;
        } else if (value.Length > expectedValueLenght) { // cut the string from beginning
            if (cutFrom == CutFrom.End)
            {
                return value.Substring(0, expectedValueLenght);
            }
            else
            {
                return value.Substring(value.Length - expectedValueLenght);
            }
        } else {
            for (int i = value.Length; i < expectedValueLenght; i++)
            {
                if (addTo == AddTo.Start)
                {
                    value = sign + value;
                }
                else
                {
                    value = value + sign;
                }
            }
        }
        return value;
    }
    public static string RemoveAbsentSignsFromTheBeginningOfTheString(string value,  char sign)
    {
        int countdigitremove = 0;
            for (int i = 0; i < value.Length-1; i++)//one digit alwas stay
            {
                if (value[i] == sign)
                    countdigitremove++;
                else break;
            }
            if (countdigitremove == 0)
                return value;
            else return 
                value.Remove(0, countdigitremove);
    }

    public static string AddAbsentSignsToTheEndOfTheString(string value, int expectedValueLength, string sign)
    {
        for (int i = value.Length; i < expectedValueLength; i++)
        {
            value += sign;
        }
        return value;
    }

    public static bool CheckSecurity(int resourceID)
    {
        int clerkID = 0;
        bool result = false;
        AgencyUser user = new AgencyUser();
		try{
        int.TryParse(user.AgencyUserId, out clerkID);

        /*aviran 23/08 - for working localy  - for production remove the comment from "Agency2000WS.Agency2000WS ss = new Agency2000WS.Agency2000WS();"*/
        Agency2000WS.Agency2000WS ss = new Agency2000WS.Agency2000WS();
        /* /aviran */

        //Logger.Log("Checking security for user id:" + clerkID.ToString() + " for resource: " + resourceID.ToString() + " "+ "error");
        //result = ss.CheckSecurity(resourceID, clerkID);

        /*aviran 23/08 - for working localy - for production remove the "result = true;" and the comment from "result = ss.CheckSecurity(int.Parse(user.AgencyId), int.Parse(user.AgencySystemType), user.AgencyUserName, user.AgencyUserPassword, resourceID);"*/
        //result = true;
        result = ss.CheckSecurity(int.Parse(user.AgencyId), int.Parse(user.AgencySystemType), user.AgencyUserName, user.AgencyUserPassword, resourceID);
        /* /aviran */

       // Logger.Log("result " + result.ToString());
		}
		catch(Exception ex){
			result = false;
		     Logger.Log("Exception. result " + ex.Message);
		}
        return result;
    }

    public static bool SendMail(string toAddress_, string data)
    {
       
        if (!ValidateString(toAddress_, true)) return false;

        if (Boolean.Parse(ConfigurationManager.AppSettings["SendMailWithNewSmtpConfig"])) return SendMailWithNewSmtp(toAddress_, data);

        return SendMailWithClassicConfig(toAddress_,data);
        
    }

    //based on Site2000 ContactUs.ascx.cs
    //for row AgencyAdmin WebConfig row: AgnAgencyId = 71, Agency_SysType = 3, AgencySubSite = 1. MailServer = 192.116.118.1 
    //here uses MailServer sent by Yossi and his mail from db
    // Currently all data in web.config appSettings as keys
    private static bool SendMailWithClassicConfig(string toAddress_, string data)
    {
        MailMessage message = new MailMessage();
        message.From = new MailAddress( ConfigurationManager.AppSettings["ClassicMessageFromMail"]);

        message.Subject = "Report " + DateTime.Now.ToShortDateString();
        message.Body = data;
        message.IsBodyHtml = Boolean.Parse( ConfigurationManager.AppSettings["ClassicMessageIsBodyHtm"] );

        try
        {
            message.To.Add(new MailAddress(toAddress_));
            SmtpClient client = new SmtpClient(ConfigurationManager.AppSettings["ClassicMailServer"]);
            client.Send(message);            
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    private static bool SendMailWithNewSmtp(string toAddress_, string data)
    {
        //define dependent vars
        string subject = "Report " + DateTime.Now.ToShortDateString();
        var toAddress = new MailAddress(toAddress_);

        //config stmp client
        var smtp = new SmtpClient(ConfigurationManager.AppSettings["NewSmtpHost"], Int32.Parse(ConfigurationManager.AppSettings["NewSmtpPort"]));
        smtp.EnableSsl = Boolean.Parse(ConfigurationManager.AppSettings["NewSmtpEnableSsl"]);
        smtp.UseDefaultCredentials = Boolean.Parse(ConfigurationManager.AppSettings["NewSmtpUseDefaultCredentials"]);
        smtp.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["NewSmtpUsername"], ConfigurationManager.AppSettings["NewSmtpPassword"]);


        //init message data and send message
        using (var message = new MailMessage()
        {
            Subject = subject,
            Body = data,
            IsBodyHtml = Boolean.Parse(ConfigurationManager.AppSettings["NewSmtpMessageIsBodyHtml"]),
            From = new MailAddress(ConfigurationManager.AppSettings["NewSmtpMessageFromAddress"])
        })
        {
            try
            {
                message.To.Add(toAddress);
                smtp.Send(message);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ValidateString(string input,bool isMail)
    {
        if (String.IsNullOrEmpty(input)) return false;
        const string patternMail = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
        const string patternPhoneFax = @"^\(?[0-9]{2}\)?[-]?([0-9]{7})$";
        string finalPattern;
        if (isMail)
        {
            finalPattern = patternMail;
        }
        else
        {
            finalPattern = patternPhoneFax;
        }
        Match m = Regex.Match(input, finalPattern);
        return m.Success;

    }
   
    public static void MelaveTashlumDefiner(int voucher_melavim, int file_melavim, int rooms, ref int be_tashlum, ref int le_be_tashlum)
    {
        be_tashlum = 0;
        le_be_tashlum = 0;
        
        if ((voucher_melavim == 0 && file_melavim == 0) || (voucher_melavim == 0 && file_melavim > 0))
        {
            be_tashlum = 0;
            le_be_tashlum = 0;
        }
        else if (voucher_melavim == 1 && file_melavim > 0)
        {
            be_tashlum = 0;
            le_be_tashlum = 1;
        }
        else if (voucher_melavim > 1 && file_melavim == 1)
        {
            be_tashlum = 0;
            le_be_tashlum = 1;
        }
        else if (voucher_melavim == 2 && file_melavim == 2)
        {
            be_tashlum = 0;
            le_be_tashlum = 2;
        }
        else if (voucher_melavim == 3 && file_melavim == 2)
        {
            be_tashlum = 1;
            le_be_tashlum = 2;
        }
        else if (voucher_melavim > 0 && file_melavim == 0)
        {
            be_tashlum = 1;
            le_be_tashlum = 0;
        }

    }

    public static string GetHotelFaxToAddress(string hotelId)
    {
        int hotelIdInt = 0;
        string fax = null;
        const string toAddressEnding = "@fax.vayosoft.com";
        

        //to avoid prefix 0 => in table no prefix
        if (Int32.TryParse(hotelId, out hotelIdInt))
        {
            if (hotelIdInt > 0)
            {
                fax = DAL_SQL.GetRecord("SUPPLIER_DETAILS", "fax", hotelIdInt.ToString(), "supplier_id");
                if (ValidateString(fax, false))
                {
                    return String.Concat(fax, toAddressEnding);
                }
            }
        }

        return String.Empty;
    }

    public static string GetFormattedDTString(DateTime dt)
    {
        return String.Format("{0}.{1}.{2}", dt.Day.ToString("00"), dt.Month.ToString("00"), dt.Year.ToString("0000"));
    }
	
	public static string getColumnValueByName(DataRow iRow, string iColumnName)
	{
		string value = string.Empty;
		
		if (iRow.Table.Columns.Contains(iColumnName))
		{
			value = iRow[iColumnName].ToString();
		}
		
		return value;
	}

    public static string GetLastdigit(string str ,int numdigit)
    {
        if (str.Length <= numdigit)
            return str;
        else
            return str.Substring(str.Length - numdigit);
    }
    public static string ConvertMonthToName(int month)
    {
        switch (month)
        {
            case 1:
                return "ינואר";//"ינואר","פברואר","מרץ","אפריל","מאי","יוני","יולי","אוגוסט","ספטמבר","אוקטובר","נובמבר","דצמבר"

            case 2:
                return "פברואר";
            case 3:
                return "מרץ";
            case 4:
                return "אפריל";
            case 5:
                return "מאי";
            case 6:
                return "יוני";
            case 7:
                return "יולי";
            case 8:
                return "אוגוסט";
            case 9:
                return "ספטמבר";
            case 10:
                return "אוקטובר";
            case 11:
                return "נובמבר";
            case 12:
                return "דצמבר";

        }
        return " ";
    }
    public static StringBuilder GetHeadelLogoReport()
    {

        StringBuilder headerTable = new StringBuilder();

        AgencyUser agencyuser = new AgencyUser();
       
        DataTable dt = DAL_SQL_Helper.GetAgencyDetails(Convert.ToInt32(agencyuser.AgencyId));
		if (dt.Rows.Count > 0)
		{
			DataRow dr = dt.Rows[0];
			headerTable.Append("<table width='98%' class='center_table' border='1' bordercolor='black' align='center' cellpadding='5' cellspacing='0' dir='rtl'>");
			headerTable.Append("<tr><td>");
			headerTable.Append("<div class='invoice_head_text'>");

        headerTable.Append("<div>");
        headerTable.Append("<div style='width: 49%;float: right; text-align: right;'> " + dr["name"].ToString() + "<BR>");// ""&rs("name")& ""

        headerTable.Append("<BR>" + "	כתובת :" + dr["address"].ToString() + "<BR>");
        headerTable.Append("	טלפון : " + dr["phone"].ToString() + "  פקס : " + dr["fax"].ToString() + "<br />");
        headerTable.Append("	.ח.פ : " + dr["id_number_het_pay"].ToString() + "  עוסק מורשה : " + dr["vat_number"].ToString() + "<br />");

			headerTable.Append("</div>");
			headerTable.Append("<div style='width: 30%;float: left; text-align: left;'>" + GetLogoImg(dr["name"].ToString(), "") + "</div>");
			headerTable.Append("</div>");
			headerTable.Append("</td></tr>");
			headerTable.Append("</table><br />");
		}
		else
		{
			headerTable.Append("<table width='98%' class='center_table' border='1' bordercolor='black' align='center' cellpadding='5' cellspacing='0' dir='rtl'>");
			headerTable.Append("<tr><td style='font-weight:bold; font-size:16px;'>Error while loading header. please re enter the report");
			headerTable.Append("</td></tr>");
		}
		
        return headerTable;
    }
    public static String GetLogoImg(string AgencyName, string ext)
    {
        string picPath;
        AgencyUser agencyuser = new AgencyUser();
        if (agencyuser.AgencyId.Length == 1)
            agencyuser.AgencyId = '0' + agencyuser.AgencyId;
        if (ext.Length == 0)
            ext = ".gif";
        picPath = "/images/Logo/logo" + agencyuser.AgencyId + ext;
        return "<IMG ALIGN='right' SRC=" + picPath + "  ALT='" + AgencyName + "' height='100px' WIDTH='190px'>";
       
    }
	
	public static string convertToAgencyXml(string iFinalPricesXml, string iAgencyId, string iSystemType, string iPriceType, string iAreaId, DateTime iFromDate, DateTime iToDate, string iRoomsAmount, string iLanguage)
	{
		XmlDocument xml = new XmlDocument();
		XmlDocument finalPriceXml = new XmlDocument();
		iFinalPricesXml = "<Root>" + iFinalPricesXml + "</Root>";
		finalPriceXml.LoadXml(iFinalPricesXml);
		string supplierId;
		string totalBrutto, totalNetto;
		string compositionId, baseId, roomTypeId, roomAmountPrice, compositionName, baseName, roomTypeName, baseAmount;
		string currency, commission, officeDiscount, siteDiscount;
		XmlDocument tempFinalPrice = new XmlDocument();
		
		foreach (XmlNode finalPrice in finalPriceXml.SelectNodes("//Root//FinalPrices//FinalPrice"))
		{
            tempFinalPrice = new XmlDocument();
            tempFinalPrice.LoadXml(finalPrice.OuterXml);
			
			if (tempFinalPrice.SelectSingleNode("//FinalPrice//HasError").InnerText.ToLower() != "true")
			{
				supplierId = tempFinalPrice.SelectSingleNode("//FinalPrice//SupplierId").InnerText;
				totalBrutto =  tempFinalPrice.SelectSingleNode("//FinalPrice//FinalPriceBrutto").InnerText;
				totalNetto =  tempFinalPrice.SelectSingleNode("//FinalPrice//FinalPriceNetto").InnerText;
				compositionId =  tempFinalPrice.SelectSingleNode("//FinalPrice//Composition//Id").InnerText;
				compositionName =  tempFinalPrice.SelectSingleNode("//FinalPrice//Composition//Name").InnerText;
				baseId =  tempFinalPrice.SelectSingleNode("//FinalPrice//Base//Id").InnerText;
				baseName =  tempFinalPrice.SelectSingleNode("//FinalPrice//Base//Name").InnerText;
				baseAmount =  tempFinalPrice.SelectSingleNode("//FinalPrice//Base//Amount").InnerText;
				roomTypeId =  tempFinalPrice.SelectSingleNode("//FinalPrice//RoomType//Id").InnerText;
				roomTypeName =  tempFinalPrice.SelectSingleNode("//FinalPrice//RoomType//Name").InnerText;
				roomAmountPrice =  tempFinalPrice.SelectSingleNode("//FinalPrice//RoomType//Amount").InnerText;
				currency = tempFinalPrice.SelectSingleNode("//FinalPrice//Currency").InnerText;
				commission = tempFinalPrice.SelectSingleNode("//FinalPrice//Commission").InnerText;
				officeDiscount = tempFinalPrice.SelectSingleNode("//FinalPrice//OfficeDiscount").InnerText;
				siteDiscount = tempFinalPrice.SelectSingleNode("//FinalPrice//SiteDiscount").InnerText;
				
				const string empty = "";
				const string zero = "0";
				const string roomState = "V";
				const string sourceType = "1";
				int index = 1;

                

                XmlElement rowData = null;
                if (xml.DocumentElement == null)
                {
                    rowData = (XmlElement)xml.AppendChild(xml.CreateElement("ROW_DATA"));
                }
                else
                {
                    rowData = (XmlElement)xml.DocumentElement.AppendChild(xml.CreateElement("ROW_DATA"));
                }
                XmlElement baseTypes = (XmlElement)rowData.AppendChild(xml.CreateElement("BASE_TYPES"));
				XmlElement baseType = (XmlElement)baseTypes.AppendChild(xml.CreateElement("BASE_TYPE"));
				XmlElement roomsInfo = (XmlElement)rowData.AppendChild(xml.CreateElement("ROOMS_INFO"));
				XmlElement room = (XmlElement)roomsInfo.AppendChild(xml.CreateElement("ROOM"));
				//FinalPrice
				rowData.AppendChild(xml.CreateElement("AGENCY_ID")).InnerText = iAgencyId;
				rowData.AppendChild(xml.CreateElement("PRICE_ID")).InnerText = "-1";
				rowData.AppendChild(xml.CreateElement("PRICE_NAME")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("HOTEL_SUPP_ID")).InnerText = supplierId;
				rowData.AppendChild(xml.CreateElement("SECOND_PRICE_NAME")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("AREA_ID")).InnerText = iAreaId;
				rowData.AppendChild(xml.CreateElement("GENERAL_AREA_NAME")).InnerText = DAL_SQL.GetRecord("Agency_Admin.dbo.GENARAL_AREAS", "name_" + iLanguage, "id", iAreaId);
				rowData.AppendChild(xml.CreateElement("AREA_NAME")).InnerText = DAL_SQL.GetRecord("Agency_Admin.dbo.AREAS", "name", "general_area_id", iAreaId);
				rowData.AppendChild(xml.CreateElement("GENERAL_AREA_ID")).InnerText = iAreaId;
				
				rowData.AppendChild(xml.CreateElement("FROM_DATE")).InnerText = iFromDate.ToString("dd-MMM-yy");
				rowData.AppendChild(xml.CreateElement("TO_DATE")).InnerText = iToDate.ToString("dd-MMM-yy");
				rowData.AppendChild(xml.CreateElement("PRICE_TYPE")).InnerText = iPriceType;
				rowData.AppendChild(xml.CreateElement("PRICE_TYPE_NAME")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("AMOUNT_ROOMS")).InnerText = iRoomsAmount;
				rowData.AppendChild(xml.CreateElement("SUPP_PRICE_ID")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("CHILD_ADD_AMOUNT")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("AMOUNT_BRUTO")).InnerText = totalBrutto;
				rowData.AppendChild(xml.CreateElement("AMOUNT_NETTO")).InnerText = totalNetto;
				rowData.AppendChild(xml.CreateElement("BASE_ID")).InnerText = baseId;
				rowData.AppendChild(xml.CreateElement("BASE_AMOUNT")).InnerText = baseAmount;
				rowData.AppendChild(xml.CreateElement("BASE_NAME")).InnerText = baseName;
				//Base types
				//TODO: foreach?
					baseType.AppendChild(xml.CreateElement("ID")).InnerText = baseId;
					baseType.AppendChild(xml.CreateElement("NAME")).InnerText = baseName;
					baseType.AppendChild(xml.CreateElement("ADULT_AMOUNT_NETTO")).InnerText = zero;
					baseType.AppendChild(xml.CreateElement("ADULT_AMOUNT_BRUTO")).InnerText = zero;
					baseType.AppendChild(xml.CreateElement("CHILD_AMOUNT_NETTO")).InnerText = zero;
					baseType.AppendChild(xml.CreateElement("CHILD_AMOUNT_BRUTO")).InnerText = zero;
					baseType.AppendChild(xml.CreateElement("KID_AMOUNT_NETTO")).InnerText = zero;
					baseType.AppendChild(xml.CreateElement("KID_AMOUNT_BRUTO")).InnerText = zero;
				baseTypes.AppendChild(baseType);
				rowData.AppendChild(baseTypes);
						
				rowData.AppendChild(xml.CreateElement("ROOM_TYPE_ID")).InnerText = compositionId; //RoomType in ageny is composition
				rowData.AppendChild(xml.CreateElement("ROOM_AMOUNT")).InnerText = compositionId; //TODO: ? not so clear what is that row
				rowData.AppendChild(xml.CreateElement("ROOM_TYPE_NAME")).InnerText = compositionName;
				rowData.AppendChild(xml.CreateElement("SUPP_COMM")).InnerText = commission;
				rowData.AppendChild(xml.CreateElement("SUPP_INCOME_TYPE")).InnerText = zero; //TODO: get SUPP_INCOME_TYPE
				rowData.AppendChild(xml.CreateElement("SUPP_IS_VAT")).InnerText = zero; //TODO: get SUPP_IS_VAT
				rowData.AppendChild(xml.CreateElement("AMOUNT_NIS")).InnerText = totalBrutto; //TODO: all the same as netto for now
				rowData.AppendChild(xml.CreateElement("AMOUNT_USD")).InnerText = totalBrutto; //TODO: all the same as netto for now
				rowData.AppendChild(xml.CreateElement("AMOUNT_EUR")).InnerText = totalBrutto; //TODO: all the same as netto for now
				rowData.AppendChild(xml.CreateElement("AMOUNT_PND")).InnerText = totalBrutto; //TODO: all the same as netto for now
				rowData.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_NIS")).InnerText = totalNetto;//TODO: all the same as brutto for now
				rowData.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_USD")).InnerText = totalNetto;//TODO: all the same as brutto for now
				rowData.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_EUR")).InnerText = totalNetto;//TODO: all the same as brutto for now
				rowData.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_PND")).InnerText = totalNetto;//TODO: all the same as brutto for now
				rowData.AppendChild(xml.CreateElement("SUPP_IMAGE_NAME")).InnerText = supplierId + "_1.jpg";
				rowData.AppendChild(xml.CreateElement("SUPP_MOVIE_NAME")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("ROOMS_STATE")).InnerText = roomState;
				rowData.AppendChild(xml.CreateElement("PRICE_DATE_RANGE_STATUS")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("PRICE_COLOR")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("PRICE_DOCKET_ID")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("PRICE_CURRENCY_ID")).InnerText = currency;
				rowData.AppendChild(xml.CreateElement("HOTEL_PRICE_INCENTIVE")).InnerText = zero;
				rowData.AppendChild(xml.CreateElement("GENERAL_INTERNAL")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("ADD_ID")).InnerText = roomTypeId;    //Roomtypes are adds in agency
				rowData.AppendChild(xml.CreateElement("ADD_NAME")).InnerText = roomTypeName;//Roomtypes are adds in agency
				//price per nights
				foreach (XmlNode finalPricePerDay in tempFinalPrice.SelectSingleNode("//FinalPrice//FinalPricesPerDays"))
				{
					XmlElement night = (XmlElement)rowData.AppendChild(xml.CreateElement("Night_" + index));
					night.AppendChild(xml.CreateElement("AmountNetto")).InnerText = finalPricePerDay.SelectSingleNode("Price").InnerText;
					//night.AppendChild(xml.CreateElement("AmountBruto")).InnerText = finalPricePerDay.SelectSingleNode("PriceNetto").InnerText;
					night.AppendChild(xml.CreateElement("AmountBruto")).InnerText = zero; // TODO: get netto like the line above.
					
					index++;
				}
				
				//Roomtypes are adds in agency
				rowData.AppendChild(xml.CreateElement("SUPP_ADD_TB")).InnerText = empty;
					//rowData.AppendChild(xml.CreateElement("SUPPLIER_ADDS")).InnerText = mErrorDescription;
					
				rowData.AppendChild(xml.CreateElement("PRICE_REMARK")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("SUPP_IMAGE_NAME1")).InnerText = supplierId + "_2.jpg";
				rowData.AppendChild(xml.CreateElement("SUPP_IMAGE_NAME2")).InnerText = supplierId + "_3.jpg";
				rowData.AppendChild(xml.CreateElement("SUPP_IMAGE_NAME3")).InnerText = supplierId + "_4.jpg";
				//Set hotel details
				//rowData.AppendChild(xml.ImportNode(getHotelInfoAgencyXml(supplierId, iLanguage);, true));
				string langStrForAgency = (iLanguage == "1255") ? "heb" : (iLanguage == "1251") ? "eng" : "rus";
				DataSet ds = DAL_SQL.RunSqlDataSet("SELECT name_" + iLanguage + ", rank_id, fax, phone, email, remark, what_in_hotel_" + langStrForAgency + ", what_in_room_" + langStrForAgency + " FROM Agency_Admin.dbo.SUPPLIERS WHERE id = " + supplierId);
				
				if (Utils.isDataSetRowsNotEmpty(ds))
				{
					DataRow row = ds.Tables[0].Rows[0];
					
					rowData.AppendChild(xml.CreateElement("HOTEL_NAME")).InnerText = row["name_" + iLanguage].ToString();
					rowData.AppendChild(xml.CreateElement("HOTEL_RANK")).InnerText = row["rank_id"].ToString();
					rowData.AppendChild(xml.CreateElement("SUPP_REMARK")).InnerText = row["remark"].ToString();
					rowData.AppendChild(xml.CreateElement("IN_HOTEL_REMARK")).InnerText = row["what_in_hotel_" + langStrForAgency].ToString();
					rowData.AppendChild(xml.CreateElement("IN_ROOM_REMARK")).InnerText = row["what_in_room_" + langStrForAgency].ToString();
					rowData.AppendChild(xml.CreateElement("HOTEL_PHONE")).InnerText = row["phone"].ToString();
					rowData.AppendChild(xml.CreateElement("HOTEL_ORDERS_PHONE")).InnerText = row["phone"].ToString();
					rowData.AppendChild(xml.CreateElement("HOTEL_FAX")).InnerText = row["fax"].ToString();
					rowData.AppendChild(xml.CreateElement("HOTEL_EMAIL")).InnerText = row["email"].ToString();
				}
				
				rowData.AppendChild(xml.CreateElement("GENERAL_PRICE_REMARK")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("SITE_COMM")).InnerText = zero;
				rowData.AppendChild(xml.CreateElement("SITE_INCENTIVE")).InnerText = zero;
				rowData.AppendChild(xml.CreateElement("HOTEL_ADDRESS")).InnerText = empty;
				rowData.AppendChild(xml.CreateElement("SALE")).InnerText = iAgencyId + "_" + supplierId;
				rowData.AppendChild(xml.CreateElement("SOURCE_TYPE")).InnerText = sourceType;
				
				//Rooms info, rooms
				//TODO: big mess should understand?
					room.AppendChild(xml.CreateElement("AMOUNT_BRUTO")).InnerText = roomAmountPrice;
					room.AppendChild(xml.CreateElement("AMOUNT_NETTO")).InnerText = zero; //TODO? has no price netto..
					room.AppendChild(xml.CreateElement("ROOM_TYPE_ID")).InnerText = compositionId;
					room.AppendChild(xml.CreateElement("ROOM_AMOUNT")).InnerText = compositionId; //TODO: ? not so clear what is that row
					room.AppendChild(xml.CreateElement("ROOM_TYPE_NAME")).InnerText = compositionName;
					room.AppendChild(xml.CreateElement("AMOUNT_NIS")).InnerText = totalBrutto;
					room.AppendChild(xml.CreateElement("AMOUNT_PND")).InnerText = totalBrutto;
					room.AppendChild(xml.CreateElement("AMOUNT_USD")).InnerText = totalBrutto;
					room.AppendChild(xml.CreateElement("AMOUNT_EUR")).InnerText = totalBrutto;
					room.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_NIS")).InnerText = totalNetto;
					room.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_USD")).InnerText = totalNetto;
					room.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_EUR")).InnerText = totalNetto;
					room.AppendChild(xml.CreateElement("AFT_DISCOUNT_AMOUNT_PND")).InnerText = totalNetto;
					room.AppendChild(xml.CreateElement("ADD_ID")).InnerText = roomTypeId;
					room.AppendChild(xml.CreateElement("AMOUNT_NETTO_BEFORE")).InnerText = totalNetto;
				roomsInfo.AppendChild(room);
				rowData.AppendChild(roomsInfo);
			}
		}
		
		return "<ROOT>" + xml.OuterXml + "</ROOT>";
	}

	public static bool IsDataSetRowsNotEmpty(DataSet iDataSet)
    {
        return isDataSetRowsNotEmpty(iDataSet);
    }
	
	public static bool isDataSetRowsNotEmpty(DataSet iDataSet)
    {//Check if the DataSet has rows in first table.
        bool isNotEmpty = false;

        if (iDataSet != null && iDataSet.Tables != null && iDataSet.Tables.Count > 0
                && iDataSet.Tables[0].Rows != null && iDataSet.Tables[0].Rows.Count > 0)
        {
            isNotEmpty = true;
        }

        return isNotEmpty;
    }

    public static string getHotelPriceId(string iSupplierId)
    {
        DataSet ds = DAL_SQL.RunSqlDataSet("SELECT id FROM P_HOTEL_PRICES WHERE supplier_id = " + iSupplierId);
        string supplierPriceId = string.Empty;

        if (isDataSetRowsNotEmpty(ds))
        {
            supplierPriceId = ds.Tables[0].Rows[0]["id"].ToString();
        }

        return supplierPriceId;
    }
}

public enum CutFrom
{
    Start,
    End
}
public enum AddTo
{
    Start,
    End
}
