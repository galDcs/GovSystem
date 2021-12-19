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
using System.IO;
using System.Text.RegularExpressions;

public partial class IPSReleaseReport : System.Web.UI.Page
{
    public StringBuilder dropdown = new StringBuilder();
    public static StringBuilder resultTable = new StringBuilder();
    public int index = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
		setAgencyData();
        
		//if (!Utils.CheckSecurity(306)) 
		//{
		//	Response.Redirect("AccessDenied.aspx?code=306");
		//}
       
        if (!Page.IsPostBack)
        {
            resultTable = new StringBuilder();
            loadInitialData();
        }
    }

    protected void ShowReport_OnClick(object sender, EventArgs e)
    {
        resultTable = new StringBuilder();
        loadReportData();
    }
	
	private void setAgencyData()
    {
        string agencyId = Request.Cookies["Agency2000"]["AgencyID"]; 
        DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", agencyId.PadLeft(4, '0'));
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", "INN");
        //string loginName = Request.Cookies["Agency2000"]["LoginName"];
		//mClerkId = DAL_SQL.GetRecord("CLERKS", "id", "login_name", "'" + loginName + "'");
    }

    private void loadInitialData()
    {
        //Create dropdown
        //dropdown.Append("<select id=\"Hotels\" name=\"Hotels\">");
        DataSet ds1 = DAL_SQL_Helper.GetHotels();
        ds1.Tables[0].Columns.Add("NameAndArea", typeof(string), "name +' - '+description");
        DataRow AllRow = ds1.Tables[0].NewRow();
        AllRow[0] = 0;
        AllRow[1] = "הכל";
        AllRow[2] = "הכל";
        AllRow[3] = "";
        ds1.Tables[0].Rows.InsertAt(AllRow, 0);
        Hotels.DataSource = ds1.Tables[0];
        Hotels.DataBind();
    }

    public void loadReportData()
    {
        resultTable = new StringBuilder();
		string trDesign = "<tr style='background-color:#dcdcdc;'>";
		int counter = 0;
		string lastHotel = string.Empty;
		string lastDocketId = string.Empty;
		bool canWatchDocket = Utils.CheckSecurity(103);
		try
		{
			if (txtFromDate.Text.Length > 0 && txtToDate.Text.Length > 0)
			{
				int total_rooms = 0;
				DateTime fromDate = DateTime.Parse(txtFromDate.Text);
				DateTime toDate = DateTime.Parse(txtToDate.Text);
				string HotelId = Hotels.SelectedValue;
				resultTable.Append(Utils.GetHeadelLogoReport());
				resultTable.Append("<table width=\"100%\" dir=\"rtl\" class=\"trans\" border='1' id='ReportTable'>");
				resultTable.Append("<tr>");
				resultTable.Append("<td colspan=\"" + 12 + "\" style=\"border-bottom: solid 1px gray;\">");
				resultTable.Append("<table dir=\"rtl\">");
				resultTable.Append("<tr>");
				resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">דו''ח הזמנות נופש</td>");
				resultTable.Append("</tr>");
				resultTable.Append("</table>");
				resultTable.Append("</td>");
				resultTable.Append("</tr>");

				resultTable.Append("<tr style=\"border: solid 0px black;\">");
				resultTable.Append("<td colspan=\"" + 12 + "\" style=\"border: solid 0px black;\">");
				resultTable.Append("<table  dir=\"rtl\" style=\"border: solid 0px black;\">");
				resultTable.Append("<tr style=\"border: solid 0px black;\">");
				resultTable.Append("<td style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
				resultTable.Append("<td style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString( toDate )+ "</td>");
				resultTable.Append("<td style=\"border: solid 0px black;\">מלון" + " : " + Hotels.SelectedItem.Text + "</td>");
				resultTable.Append("</tr>");
				resultTable.Append("</table>");
				resultTable.Append("</td>");
				resultTable.Append("</tr>");

				string buttonHotel = string.Empty;
				string areaName = string.Empty;
				int datediff = toDate.Subtract(fromDate).Days;				
				DataSet ds2 = DAL_SQL_Helper.GetCurrentAllocationsForReportOrdersByDateRange(fromDate,toDate, Convert.ToInt32(HotelId));
			  
				foreach (DataRow row in ds2.Tables[0].Rows)
				{
					areaName = DAL_SQL.GetRecord("Agency_Admin.dbo.AREAS", "name_1255", row["area_id"].ToString(), "id");

					string vacationType = DAL_SQL.GetRecord("IPS_TRAVELLERS", "SUG_NOFESH", "IDNT_ZEHUT", row["id_no"].ToString());
					
					//TODO: what about zakaim from past year? got no SUG_NOFESH and got order
					if  (vacationType == ddlVacationType.SelectedValue || ddlVacationType.SelectedValue == "none")
					{
						// Set each hotel in other table in order to print every hotel in other page.
						if (row["hotelId"].ToString() != lastHotel)
						{						
							resultTable.Append("<br>");
							resultTable.Append("<br>");
							index++;                   
							resultTable.Append("<table width=\"100%\" dir=\"rtl\" class=\"trans\" border='1' style='page-break-before: always;'>");
								resultTable.Append("<tr id='headerForPrint_" + index + "' style=\"display:none; border: solid 0px black;\">");
									resultTable.Append("<td colspan=\"" + 12 + "\" style=\"border-bottom: solid 1px gray; \">");
									resultTable.Append(Utils.GetHeadelLogoReport());
									resultTable.Append("<table  dir=\"rtl\" style=\"border: solid 0px black;\">");
										resultTable.Append("<tr style=\"border: solid 0px black;\">");
										resultTable.Append("<td style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString(fromDate) + "</td>");
										resultTable.Append("<td style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(toDate) + "</td>");
										resultTable.Append("</tr>");

										resultTable.Append("<tr style=\"border: solid 0px black;\">");
										resultTable.Append("</tr>");
									resultTable.Append("</td>");
								resultTable.Append("</tr>");
							resultTable.Append("</table>");

							resultTable.Append("<br>");
							resultTable.Append("<br>");
							resultTable.Append("<tr><th colspan='12'><h2>" + row["HotelName"] + " " + areaName + "</h2></th></tr>");
							resultTable.Append("<tr>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>שם מלון</th>");
							//resultTable.Append("<th style='background-color:#9e9e9e; '>מס. שובר</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>מס. תיק</th>");						
							resultTable.Append("<th style='background-color:#9e9e9e; '>תאריך</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>תאריך עזיבה</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>מספר לילות</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>בסיס ארוח</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>חדרים</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>סוג נופש</th>");
							
							//resultTable.Append("<th style='background-color:#9e9e9e; '>הרכב</th>");
							resultTable.Append("<th style='background-color:#9e9e9e; '>שם אורח</th>");
							
							//Remove cause this report is only for shabas. they are not suppose to see the remarks.
							//resultTable.Append("<th style='background-color:#9e9e9e; '>הערות לספק</th>");
							resultTable.Append("</tr>");
						}
					
						if(counter % 2 == 0)
						{
							trDesign = "<tr style='background-color:#eaeaea; text-align:center;font-family:arial;'>";
						}	
						else
						{
							trDesign = "<tr style='background-color:#ffffff;text-align:center;font-family:arial;'>";
						}
						
						counter++;
						resultTable.Append(trDesign);
						resultTable.Append("<td style='text-align:center; /*height:60px !important; */'>" + row["HotelName"] + " " + areaName + "</td>");
						//resultTable.Append("<td style='width:2%; text-align:center;'>" + row["VaucherId"] + "</td>");
						//if (canWatchDocket)
						//{
						//	resultTable.Append("<td style='width:5%; text-align:center;'><a TARGET='_blank' href='"+ConfigurationManager.AppSettings.Get("AgencyDocketLink") + row["docket_id"] +"&PageFunc=common'>"+ row["docket_id"] + "</a></td>");
						//}
						//else
						{
							resultTable.Append("<td style='width:5%; text-align:center;'>"+ row["docket_id"] + "</td>");
						}
						
						resultTable.Append("<td style='/*width:8%;*/ text-align:center;' dir='ltr' >" + DateTime.Parse(row["from_date"].ToString()).ToShortDateString() + "</td>");
						resultTable.Append("<td style='/*width:8%;*/ text-align:center;' dir='ltr' >" + DateTime.Parse(row["to_date"].ToString()).ToShortDateString() + "</td>");
						resultTable.Append("<td style='/*width:1%;*/ text-align:center;'>" + row["DaysNumber"] + "</td>");
						resultTable.Append("<td style='/*width:8%;*/ text-align:center;' align='right'>" + row["BaseName"] + "</td>");
						if (int.Parse(row["RoomsSum"].ToString()) > 1)
						{
							resultTable.Append("<td style='width:1%; text-align:center;'>1/" + row["RoomsSum"] + "</td>");
						}
						else
						{
							resultTable.Append("<td style='width:1%; text-align:center;'>" + row["RoomsSum"] + "</td>");
						}	
						
						//IPS 
						vacationType = (vacationType == "מ") ? "מבצעי" : (vacationType == "ת") ? "תקציבי" : "ללא זכאות נופש";
						resultTable.Append("<td style='width:1%; text-align:center;'>" + vacationType + "</td>");
						//resultTable.Append("<td style='text-align:center;' align='right'>" + row["RoomName"] + "</td>");
						resultTable.Append("<td style='width:11%; text-align:center;' align='right'>" + row["TravellerFullName"] + "</td>");
						
						//Remove cause this report is only for shabas. they are not suppose to see the remarks.
						//resultTable.Append("<td style='width:11%; text-align:center;' align='right'>" + row["remark"] + "</td>");
						
						
				//Not relevant for IPS
						//if (lastDocketId != row["docket_id"].ToString() || !string.IsNullOrEmpty(lastDocketId ))
						//{
						//	string attachedEsortDetails = string.Empty;
						//	if (row["melave_selected_nights"] != null)
						//	{
						//		string escortNights = row["melave_selected_nights"].ToString();
						//		
						//		if (escortNights.Contains("1")) //Means seleted date for escort.
						//		{
						//			DateTime from = DateTime.Parse(row["from_date"].ToString());
						//			DateTime to = DateTime.Parse(row["to_date"].ToString());
						//			int diff = (to - from).Days;
						//
						//			attachedEsortDetails += "תקופת מלווה: ";
						//			for (int i = 0; i < diff; i++) 
						//			{
						//				if (escortNights[i] == '1')
						//				{
						//					if (i != diff - 1)
						//					{
						//						attachedEsortDetails += (from.ToString("dd/MM/yy") + ", ");
						//					}
						//					else
						//					{
						//						attachedEsortDetails += (from.ToString("dd/MM/yy"));
						//					}
						//				}
						//				from = from.AddDays(1);
						//			}
						//		}
						//	}
						//
						//	if (string.IsNullOrEmpty(attachedEsortDetails))
						//	{
						//		resultTable.Append("<td align='right' style='width:14%;font-size:11px;text-align:right;padding-right:10px;'>" + row["remark"] + attachedEsortDetails + "</td>");
						//	}
						//	else
						//	{
						//		resultTable.Append("<td align='right' style='width:14%; font-weight:bold; font-size:13px; text-align:right;padding-right:10px;'>" + row["remark"] + attachedEsortDetails + "</td>");
						//	}
						//}
						//else
						//{
						//	Logger.Log("here2");
						//	resultTable.Append("<td align='right'>" + string.Empty + "</td>");
						//}
						
						resultTable.Append("</tr>");

						if (row["RoomName"].ToString().Length > 0)
							total_rooms++;

						//Save last hotel name. to print in fidderent pages.
						lastHotel = row["hotelId"].ToString();
						
						lastDocketId = row["docket_id"].ToString();
						
						//Close specific hotel table. 
						if (row["hotelId"].ToString() != lastHotel)
						{
							resultTable.Append("</table>");
						}
					}
                }
            //}
            resultTable.Append("<tr><td>Total</td><td colspan='12'>" + total_rooms + "</td></tr>");

            resultTable.Append("</table>");

            //DataSet ds1 = DAL_SQL_Helper.GetBCInvoicesForInvoicesFile(fromDate, toDate);
        }
		}catch(Exception ex)
		{
			Logger.Log(ex.Message);
		}
    }

    protected void btnToXls_Click(object sender, EventArgs e)
    {
        //StreamReader streamReader = new StreamReader(Server.MapPath("App_Themes/Theme1/StyleSheet.css"));
        //string style = streamReader.ReadToEnd();
        //style = @"<style>" + style + "</style> ";
        //streamReader.Close();

        Response.Clear();
        Response.ContentEncoding = Encoding.Unicode;
        Response.BinaryWrite(Encoding.Unicode.GetPreamble());
        Response.AddHeader("content-disposition", "attachment;filename=ReportExcel.xls");
        Response.ContentType = "application/ms-excel;";
        StringWriter sw = new StringWriter();
        //Response.Write(style);
        Response.Write(resultTable.ToString());
        Response.End();
    }
    protected void btnMail_Click(object sender, EventArgs e)
    {
        string toAddress = hdnFMail.Value;
       
        bool res = Utils.SendMail(toAddress, resultTable.ToString());
        hdnFRes.Value = res.ToString();        
    }
    protected void btnFax_Click(object sender, EventArgs e)
    {
        string sup_id = Hotels.SelectedValue;

        if (!String.IsNullOrEmpty(sup_id) && sup_id != "0")
        {
            string toAddress = Utils.GetHotelFaxToAddress(Hotels.SelectedValue);
          
            bool res = Utils.SendMail(toAddress, resultTable.ToString());        
            hdnFFax.Value = res.ToString();
        }

    }
}
