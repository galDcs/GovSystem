using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;

public partial class NegativeSubsidReport : System.Web.UI.Page
{
    public static StringBuilder resultTable = new StringBuilder();

    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Utils.CheckSecurity(232)) Response.Redirect("AccessDenied.aspx");
        if (!IsPostBack) resultTable = new StringBuilder();
    }
    protected void ShowReport_Click(object sender, EventArgs e)
    {
       resultTable = new StringBuilder();
       loadReportData();
    }

    private void loadReportData()
    {
        if (!String.IsNullOrEmpty(txtFromDate.Text) && !String.IsNullOrEmpty(txtToDate.Text))
        {
            DateTime fromDate = new DateTime();
            DateTime toDate = new DateTime();
            if (DateTime.TryParse(txtFromDate.Text, out fromDate) && DateTime.TryParse(txtToDate.Text, out toDate))
            {
				//Baseless
				AppendHeadingsBase(fromDate, toDate);
                AppendDataBase(fromDate, toDate);
				
				//Roomless
				AppendHeadingsRooms(fromDate, toDate);
                AppendDataRooms(fromDate, toDate);	
				
				//Voucherless
				AppendHeadingsVouchers(fromDate, toDate);
                AppendDataVouchers(fromDate, toDate);					
				
				//Statuses Differences 
				AppendHeadingsStatus(fromDate, toDate);
                AppendDataStatus(fromDate, toDate);			

				//One day without indication
				//AppendHeadingsOneDayWithoutIndication(fromDate, toDate);
				//AppendDataOneDayWithoutIndication(fromDate, toDate);
				
				//Attraction less than 5 days
				AppendHeadingsAttractionWithLessThan5Days(fromDate, toDate);
				AppendDataAttractionWithLessThan5Days(fromDate, toDate);
				
				//Attraction less than 5 days
				AppendHeadingsOthersFourPlusOneLessThanFourDays(fromDate, toDate);
				AppendDataOthersFourPlusOneLessThanFourDays(fromDate, toDate);
				
				//Bundles with trav pay - Hotel
				 AppendHeadingsTravPayBundlesHotel(fromDate, toDate);
				AppendDataTravPayBundlesHotel(fromDate, toDate);
				
				//Bundles with trav pay - Hotel
				AppendHeadingsTravPayBundlesAttraction(fromDate, toDate);
				AppendDataTravPayBundlesAttraction(fromDate, toDate);
				
				//Negative Sibsud
                AppendHeadings(fromDate, toDate);
                AppendData(fromDate, toDate);
				
				
            }
        }
    }

    private void AppendData(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetNegativeSubsidAndTravPayReport(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + Math.Round(Decimal.Parse(row["amount"].ToString()), 2) + "</td>");
				resultTable.Append("<td dir='ltr'>" + Math.Round( Decimal.Parse (row["subsid"].ToString()), 2) + "</td>");
				resultTable.Append("<td dir='ltr'>" + Math.Round( Decimal.Parse (row["trav_pay"].ToString()), 2) + "</td>");            
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("<td>" + row["voucher_id"] + "</td>");
				
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadings(DateTime fromDate, DateTime toDate)
    {
        
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">סיבסוד ו\\או תשלום לקוח שליליים</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מס. תיק</th>");
        resultTable.Append("<th>כמות</th>");
        resultTable.Append("<th>סיבסוד</th>");
        resultTable.Append("<th>תשלום לקוח</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        resultTable.Append("<th>מס. שובר</th>");
        resultTable.Append("</tr>");
    }
	
	
	private void AppendDataBase(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetBaselessOrders(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["VaucherId"].ToString() + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["HotelName"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["DaysNumber"].ToString() + "</td>");            
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='6'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsBase(DateTime fromDate, DateTime toDate)
    {
        resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">הזמנות ללא הרכב</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שובר</th>");
        resultTable.Append("<th>שם מלון</th>");
        
        resultTable.Append("<th>מספר ימים</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
	
	
	private void AppendDataRooms(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetRoomlessOrders(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				//"+ row["area_name"].ToString()+ "
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["bundle_id"].ToString() + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["name"].ToString() +", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["rooms"].ToString() + "</td>");            
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("</tr>");
			}
		}
		else
		{
				resultTable.Append("<tr><td colspan='6'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsRooms(DateTime fromDate, DateTime toDate)
    {
        //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">הזמנות ללא חדר</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
        resultTable.Append("<th>שם מלון</th>");
        resultTable.Append("<th>חדרים</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
	
	private void AppendDataVouchers(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetVoucherlessOrders(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["voucher_id"].ToString() + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["supplier_name"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
				resultTable.Append("<tr><td colspan='6'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsVouchers(DateTime fromDate, DateTime toDate)
    {
        //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<br/><hr/><br/><table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">הזמנות ללא שובר</td>");
        resultTable.Append("</tr>");
		resultTable.Append("<tr>");
		resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
		resultTable.Append("הזמנות שנלחץ Change ולא נלחץ Create Voucher");
		resultTable.Append("</td>");
		resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
        resultTable.Append("<th>שם מלון</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }

    	private void AppendDataStatus(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetStatusDifferencesOrders(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["id"].ToString() + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["supplier_name"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["voucher_status"].ToString() + "</td>");     
				resultTable.Append("<td dir='ltr'>" + row["service_status_id"].ToString() + "</td>");            
				resultTable.Append("<td dir='ltr'>" + row["gov_voucher_cancel_date"].ToString() + "</td>");     
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + " - " + row["bundle_id"]+  "</td>");
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsStatus(DateTime fromDate, DateTime toDate)
    {
        //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
		resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">מלונות עם סטטוס לא מתאים לשובר</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שובר</th>");
        resultTable.Append("<th>שם מלון</th>");
		resultTable.Append("<th>סטטוס שובר</th>");
        resultTable.Append("<th>סטטוס מלון</th>");
		resultTable.Append("<th>תאריך ביטול משהבט</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
	
	private void AppendDataOneDayWithoutIndication(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetIndicationlessVouchers(fromDate, toDate);
		string serviceType = "0";
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{	serviceType = row["service_type"].ToString();
				if (serviceType == "2")
					serviceType = "מלון";
				else if (serviceType == "8")
					serviceType = "חמי מרפא";
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["id"].ToString() + "</td>");
				resultTable.Append("<td>" + row["gov_indicator_id"].ToString() + "</td>");
				resultTable.Append("<td>" + serviceType + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["supplier_name"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsOneDayWithoutIndication(DateTime fromDate, DateTime toDate)
    {
        //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">שוברים בעלי חיוב בפועל (50% או 100% מעלות יום) ללא סיבת ביטול</td>");
        resultTable.Append("</tr>");
		resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:12px;border: solid 0px black;\">יש להוסיף אינדיקציה על מנת שייצא בקובץ מימושים</td>");
        resultTable.Append("</tr>");
		resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:12px;border: solid 0px black;\"><img src='css/images/Indic.png' alt='indication in docket'/></td>");
        resultTable.Append("</tr>");
		
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
		resultTable.Append("<th>אינדיקציה</th>");
		resultTable.Append("<th>סוג שובר</th>");
        resultTable.Append("<th>שם מלון</th>");
        
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
	//////////////////////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////
	private void AppendDataAttractionWithLessThan5Days(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetAttractionLowerThenFiveDays(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["id"].ToString() + "</td>");
				resultTable.Append("<td>" + row["gov_indicator_id"].ToString() + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["supplier_name"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["date_diff"] + "</td>");
				
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsAttractionWithLessThan5Days(DateTime fromDate, DateTime toDate)
    {
        //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">חמי מרפא סטטוס OK מתחת לחמישה ימים (ללא 4+1)</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
		resultTable.Append("<th>אינדיקציה</th>");
        resultTable.Append("<th>שם מלון</th>");
        
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
		resultTable.Append("<th>מספר ימים</th>"); 
        
        resultTable.Append("</tr>");
    }
	////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////
	private void AppendDataOthersFourPlusOneLessThanFourDays(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetOthersFourPlusOneLessThanFourDays(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td>" + row["id"].ToString() + "</td>");
				resultTable.Append("<td>" + row["gov_indicator_id"].ToString() + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["supplier_name"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["date_diff"] + "</td>");
				
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsOthersFourPlusOneLessThanFourDays(DateTime fromDate, DateTime toDate)
    {
        //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">4+1 מתחת לארבעה ימים (סטטוס OK)</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
		resultTable.Append("<th>אינדיקציה</th>");
        resultTable.Append("<th>שם מלון</th>");
        
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
		resultTable.Append("<th>מספר ימים</th>"); 
        
        resultTable.Append("</tr>");
    }
	////////////////////////////////////////////////////////////////////////
	
	private void AppendDataMarkUpTaxTravPay(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetVoucherWithMarkUpOrTaxOrTravPay(fromDate, toDate);
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				resultTable.Append("<tr>");
				
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				
				resultTable.Append("<td dir='ltr'>" + row["supplier_name"].ToString() + ", "+ row["area_name"].ToString()+ "</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsMarkUpTaxTravPay(DateTime fromDate, DateTime toDate)
    {
       resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">תיקים עם MU או מיסים או הש' עובד</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr>");
        resultTable.Append("<th>מספר תיק</th>");
        
        resultTable.Append("<th>שם מלון</th>");
        
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
	
	////////////////////////////////////////////////////////////////////////
	
	private void AppendDataTravPayBundlesHotel(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetTravPayBundlesHotels(fromDate, toDate);
		int i = 0;
		
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				i++;
				
				resultTable.Append("<tr>");
				resultTable.Append("<td>"+ i +"</td>");
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["bundle_id"].ToString()+"</td>");
				resultTable.Append("<td dir='ltr'>" + row["trav_pay"].ToString()+"</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsTravPayBundlesHotel(DateTime fromDate, DateTime toDate)
    {
       //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">שורות פעילות עם הש' עובד - מלונות</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");
        resultTable.Append("<tr>");
		resultTable.Append("<th></th>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
		resultTable.Append("<th>סכום נוסע</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
	
		////////////////////////////////////////////////////////////////////////
	
	private void AppendDataTravPayBundlesAttraction(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = DAL_SQL_Helper.GetTravPayBundlesAttraction(fromDate, toDate);
		int i = 0;
		
		if (ds.Tables[0].Rows.Count > 0 )
		{
			foreach (DataRow row in ds.Tables[0].Rows)
			{
				i++;
				
				resultTable.Append("<tr>");
				resultTable.Append("<td>"+ i +"</td>");
				resultTable.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["bundle_id"].ToString()+"</td>");
				resultTable.Append("<td dir='ltr'>" + row["trav_pay"].ToString()+"</td>");
				resultTable.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
				resultTable.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
				resultTable.Append("</tr>");
	
			}
		}
		else
		{
			resultTable.Append("<tr><td colspan='7'>אין תוצאות</td></tr>");
		}
    }

    private void AppendHeadingsTravPayBundlesAttraction(DateTime fromDate, DateTime toDate)
    {
       //resultTable.Append(Utils.GetHeadelLogoReport());
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" id='myTable' class='trans'>");
        resultTable.Append("<tr>");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTable.Append("<tr>");
        resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">שורות פעילות עם הש' עובד - חמי מרפא (ללא 4+1)</td>");
        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");

        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTable.Append("<tr style=\"border: solid 0px black;\">");
        resultTable.Append("<td  style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
        resultTable.Append("<td  style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(  toDate  ) + "</td>");

        resultTable.Append("</tr>");
        resultTable.Append("</table>");
        resultTable.Append("</td>");
        resultTable.Append("</tr>");
        resultTable.Append("<tr>");
		resultTable.Append("<th></th>");
        resultTable.Append("<th>מספר תיק</th>");
        resultTable.Append("<th>מספר שורה</th>");
		resultTable.Append("<th>סכום נוסע</th>");
        resultTable.Append("<th>מתאריך</th>");
        resultTable.Append("<th>עד תאריך</th>");        
        
        resultTable.Append("</tr>");
    }
}
