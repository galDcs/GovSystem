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

public partial class Jordalish : System.Web.UI.Page
{

    public static StringBuilder resultTable = new StringBuilder();

    protected void Page_Load(object sender, EventArgs e)
    {
       
      if (!IsPostBack) resultTable = new StringBuilder();

    
    }

    protected void ShowReport_OnClick(object sender, EventArgs e)
    {
        resultTable = new StringBuilder();
        loadReportData();
    }

    private void loadReportData()
    {
		int sumOfAll = 0;
        if (txtFromDate.Text.Length > 0 && txtToDate.Text.Length > 0)
        {
            //DateTime fromDate = new DateTime(2012, 2, 1);
            //DateTime toDate = new DateTime(2012, 2, 29);

            DateTime fromDate = DateTime.Parse(txtFromDate.Text);
            DateTime toDate = DateTime.Parse(txtToDate.Text).AddDays(1);

            StringBuilder tableHeader = new StringBuilder();
            tableHeader.Append("<tr>");
            //tableHeader.Append("<th>שם מלון</th>");
            //tableHeader.Append("<th>אזור</th>");
            
            int datediff = toDate.Subtract(fromDate).Days;
            int[] totalRooms = new int[datediff];

            int TDCounterForName = 0;
            for (int i = 0; i < datediff; i++)
            {
           //     DateTime currentDate = fromDate.AddDays(i);
           //     tableHeader.Append("<th>" + currentDate.Day + "/" + currentDate.Month + "</th>");
                TDCounterForName++;
            }
           // tableHeader.Append("<th>סה\"כ</th>");
            tableHeader.Append("</tr>");

            TDCounterForName -= 1;
            resultTable.Append(Utils.GetHeadelLogoReport());
            resultTable.Append("<table width=\"95%\" dir=\"rtl\" border='1' class=\"trans\">");
            resultTable.Append("<tr>");
            resultTable.Append("<td colspan=\"" + (TDCounterForName + 4) + "\" style=\"border-bottom: solid 1px gray;\">");
            resultTable.Append("<table width=\"100%\" dir=\"rtl\" >");
            resultTable.Append("<tr>");
            resultTable.Append("<td style=\"font-size:22px;border: solid 0px black;\">דו\"ח שהייה יומי</td>");
            resultTable.Append("</tr>");
            resultTable.Append("</table>");
            resultTable.Append("</td>");
            resultTable.Append("</tr>");

            resultTable.Append("<tr style=\"border: solid 0px black;\">");
            resultTable.Append("<td colspan=\"" + (TDCounterForName + 4) + "\" style=\"border: solid 0px black;\">");
            resultTable.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
            resultTable.Append("<tr style=\"border: solid 0px black;\">");
            resultTable.Append("<td style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString( fromDate )+ "</td>");
            resultTable.Append("<td style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString ( toDate.AddDays(-1) )+ "</td>");
            resultTable.Append("</tr>");
            resultTable.Append("</table>");
            resultTable.Append("</td>");
            resultTable.Append("</tr>");

            
            resultTable.Append(tableHeader);




            DataSet ds1 = DAL_SQL_Helper.GetSuppliersForOrdersReport(fromDate, toDate);
            
            foreach (DataRow row in ds1.Tables[0].Rows)
            {
                int totalRoomsPerSupplier = 0;
				int totalRoomsAllocated = 0;
				int totalRoomsOrdered = 0;
				int colspanValue = datediff+1;

                resultTable.Append("<tr>");
                resultTable.Append("<td style='padding-top: 16px; font-family:arial; font-size:16px;' colspan="+colspanValue+" align='right'>" + row["HotelName"] + ", " + row["AreaName"] +"</td>");
				
                //esultTable.Append("<td align='right'>" + row["AreaName"] + "</td>");
				resultTable.Append("</tr>");
				 resultTable.Append("<tr>");
				 resultTable.Append("<th></th>"); 
				for (int i = 0; i < datediff; i++)
				{
					DateTime currentDate = fromDate.AddDays(i);
					resultTable.Append("<th>" + currentDate.Day + "/" + currentDate.Month + "</th>");
					
				}
				resultTable.Append("<th>סה\"כ</th>");
				resultTable.Append("</tr>");
				resultTable.Append("</tr>");
				resultTable.Append("<td>הזמנות</td>");
				StringBuilder sbOrdered = new StringBuilder();
                StringBuilder sbAllocated = new StringBuilder();
                sbAllocated.Append("<tr style='background-color: #fcd5b4;'>");
				sbAllocated.Append("<td>הקצאות</td>");
				sbOrdered.Append("<tr style='background-color: #ccc0da;'>");
				sbOrdered.Append("<td>הקצאות נלקחו</td>");
                for (int i = 0; i < datediff; i++)
                {
                    DateTime currentDate = fromDate.AddDays(i);

                    DataSet ds2 = DAL_SQL_Helper.GetRoomsForOrdersReport(currentDate,fromDate, toDate, Convert.ToInt32(row["HotelId"]));
                    if (ds2.Tables[0].Rows.Count == 0)
                    {
                        resultTable.Append("<td>0</td>");
                        //sbOrdered.Append("<td>0</td>");
                        //sbAllocated.Append("<td>0</td>");
						string roomsOrdered =  DAL_SQL.RunSql(@"SELECT (cast(rooms_ordered  as nvarchar(4))+ '|' + cast(rooms_amount as nvarchar(4))) as rooms FROM SUPPLIERS_TO_PRICES_DATES_ROOMS 
															   WHERE price_date_id in (SELECT id FROM SUPPLIERS_TO_PRICES WHERE supplier_id = " + row["HotelId"].ToString() + " AND SUPPLIERS_TO_PRICES.STATUS= 1) " +
                                                               "AND room_date = '" + currentDate.ToString("dd-MMM-yy")+"'");
						string jordyJordId = DAL_SQL.RunSql(@"SELECT id FROM SUPPLIERS_TO_PRICES_DATES_ROOMS 
															   WHERE price_date_id in (SELECT id FROM SUPPLIERS_TO_PRICES WHERE supplier_id = " + row["HotelId"].ToString() + " AND SUPPLIERS_TO_PRICES.STATUS= 1) " +
                                                               "AND room_date = '" + currentDate.ToString("dd-MMM-yy")+"'");
															   
						if (roomsOrdered.Split('|').Length != 2)
						{
							roomsOrdered = "0|0";
						}
						string ordered = roomsOrdered.Split('|')[0];
                        string allocated = roomsOrdered.Split('|')[1];
						int currentDateRoom = 0;
                        //resultTable.Append("<td title='rooms'>" + ds2.Tables[0].Rows[0]["RoomsSum"] + "</td>");
                        //totalRooms[i] += int.Parse(ds2.Tables[0].Rows[0]["RoomsSum"].ToString());
                        //totalRoomsPerSupplier += int.Parse(ds2.Tables[0].Rows[0]["RoomsSum"].ToString());
						totalRoomsOrdered += int.Parse(ordered); //int.Parse(ds2.Tables[0].Rows[0]["roomsOrdered"].ToString());
                        totalRoomsAllocated+= int.Parse(allocated); //int.Parse(ds2.Tables[0].Rows[0]["roomsAllocated"].ToString());
						
						if (currentDateRoom == int.Parse(ordered))
						{
							sbOrdered.Append("<td>" + ordered + "</td>");
						}
						else
						{
							DAL_SQL.UpdateRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "rooms_ordered", currentDateRoom.ToString(), jordyJordId, "id");
							//Logger.EmptyLog("changed from " + ordered + " to" + currentDateRoom + ".jordyJordId = " + jordyJordId + ", priceId = " + oyoYooooi + ", suppId = " + row["HotelId"].ToString() + ", date = " + currentDate.ToString("dd-MMM-yy"),eLogger.DEBUG);
								
							sbOrdered.Append("<td style='background-color:#ea6060;' title='ההקצאה לא שווה לכמות ההזמנות'>" + ordered + "</td>");
						}
						
						if (currentDateRoom <= int.Parse(allocated))
						{
							sbAllocated.Append("<td>" + allocated + "</td>");
						}
						else
						{
							sbAllocated.Append("<td style='background-color:#ff8989;' title='הוזמנו יותר ממה שהוקצה'>" + allocated + "</td>");
						}
                    }
                    else 
                    {
						string ordersOfSelectedDate = getOrdersForSelectedDate(currentDate, fromDate, toDate, Convert.ToInt32(row["HotelId"]), row["HotelId"] + "_" + i);
                        resultTable.Append("<td id=' " + row["HotelId"] + "_" + i + "' title='rooms' style='cursor:hand;' onClick=showOrders('div_" + row["HotelId"] + "_" + i +"') >" + ds2.Tables[0].Rows[0]["RoomsSum"] + "</td>");

						string roomsOrdered =  DAL_SQL.RunSql(@"SELECT (cast(rooms_ordered  as nvarchar(4))+ '|' + cast(rooms_amount as nvarchar(4))) as rooms FROM SUPPLIERS_TO_PRICES_DATES_ROOMS 
															   WHERE price_date_id in (SELECT id FROM SUPPLIERS_TO_PRICES WHERE supplier_id = " + row["HotelId"].ToString() + " AND SUPPLIERS_TO_PRICES.STATUS= 1) " +
                                                               "AND room_date = '" + currentDate.ToString("dd-MMM-yy")+"'");
					   string jordyJordId = DAL_SQL.RunSql(@"SELECT id FROM SUPPLIERS_TO_PRICES_DATES_ROOMS 
															   WHERE price_date_id in (SELECT id FROM SUPPLIERS_TO_PRICES WHERE supplier_id = " + row["HotelId"].ToString() + " AND SUPPLIERS_TO_PRICES.STATUS= 1) " +
                                                               "AND room_date = '" + currentDate.ToString("dd-MMM-yy")+"'");
						string oyoYooooi = DAL_SQL.RunSql(@"SELECT price_date_id FROM SUPPLIERS_TO_PRICES_DATES_ROOMS 
															   WHERE price_date_id in (SELECT id FROM SUPPLIERS_TO_PRICES WHERE supplier_id = " + row["HotelId"].ToString() + " AND SUPPLIERS_TO_PRICES.STATUS= 1) " +
                                                               "AND room_date = '" + currentDate.ToString("dd-MMM-yy")+"'");
															   
                        string ordered = roomsOrdered.Split('|')[0];
                        string allocated = roomsOrdered.Split('|')[1];
						int currentDateRoom = int.Parse(ds2.Tables[0].Rows[0]["RoomsSum"].ToString());
                        //resultTable.Append("<td title='rooms'>" + ds2.Tables[0].Rows[0]["RoomsSum"] + "</td>");
                        totalRooms[i] += int.Parse(ds2.Tables[0].Rows[0]["RoomsSum"].ToString());
                        totalRoomsPerSupplier += int.Parse(ds2.Tables[0].Rows[0]["RoomsSum"].ToString());
						totalRoomsOrdered += int.Parse(ordered); //int.Parse(ds2.Tables[0].Rows[0]["roomsOrdered"].ToString());
                        totalRoomsAllocated+= int.Parse(allocated); //int.Parse(ds2.Tables[0].Rows[0]["roomsAllocated"].ToString());
						
						if (currentDateRoom == int.Parse(ordered))
						{
							sbOrdered.Append("<td>" + ordered + "</td>");
						}
						else
						{
							sbOrdered.Append("<td style='background-color:#ea6060;' title='ההקצאה לא שווה לכמות ההזמנות'>" + ordered + "</td>");
							
								DAL_SQL.UpdateRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "rooms_ordered", currentDateRoom.ToString(), jordyJordId, "id");
								Logger.EmptyLog("changed from " + ordered + " to" + currentDateRoom + ".jordyJordId = " + jordyJordId + ", priceId = " + oyoYooooi + ", suppId = " + row["HotelId"].ToString() + ", date = " + currentDate.ToString("dd-MMM-yy"),eLogger.DEBUG);
							
							
						}
						
						if (currentDateRoom <= int.Parse(allocated))
						{
							sbAllocated.Append("<td>" + allocated + "</td>");
						}
						else
						{
							sbAllocated.Append("<td style='background-color:#ff8989;' title='הוזמנו יותר ממה שהוקצה'>" + allocated + "</td>");
						}
                    }
                }
                
                resultTable.Append("<th title='roomsPerSupplier'>"+ totalRoomsPerSupplier.ToString() +"</th>");
                resultTable.Append("</tr>");
                sbAllocated.Append("<th>"+totalRoomsAllocated+"</th></tr>");
                sbOrdered.Append("<th>"+totalRoomsOrdered+"</th></tr>");
                resultTable.Append(sbOrdered.ToString());
				resultTable.Append(sbAllocated.ToString());
				resultTable.Append("<tr><td style=' /*background: #ebf1f6; Old browsers */background: -moz-linear-gradient(top, #ebf1f6 0%, #89c3eb 0%, #abd3ee 50%, #d5ebfb 100%); /* FF3.6-15 */background: -webkit-linear-gradient(top, #ebf1f6 0%,#89c3eb 0%,#abd3ee 50%,#d5ebfb 100%); /* Chrome10-25,Safari5.1-6 */background: linear-gradient(to bottom, #ebf1f6 0%,#89c3eb 0%,#abd3ee 50%,#d5ebfb 100%); /* W3C, IE10+, FF16+, Chrome26+, Opera12+, Safari7+ */' colspan="+colspanValue+">~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~</td></tr>");
				sumOfAll += totalRoomsPerSupplier;
            }


            resultTable.Append("<tr>");
            resultTable.Append("<td colspan=\"" + (TDCounterForName + 4) + "\">סה\"כ</td>");
			resultTable.Append("</tr>");
			resultTable.Append("<tr>");
			for (int i = 0; i < datediff; i++)
			{
				DateTime currentDate = fromDate.AddDays(i);
				resultTable.Append("<th>" + currentDate.Day + "/" + currentDate.Month + "</th>");
			}
			resultTable.Append("<th>סה\"כ</td></th>");
			resultTable.Append("<tr>");
            for (int num = 0; num < totalRooms.Length; num++)
            {
				
                resultTable.Append("<td title='rooms'>"+ totalRooms[num].ToString() +"</td>");
            }
            resultTable.Append("<th>"+sumOfAll+"</th>");
            resultTable.Append("</tr>");

            resultTable.Append("</table>");
        }
    }
	
	 //Build an html with table of orders of clicked date.
    private string getOrdersForSelectedDate(DateTime iCurrentDate, DateTime iFromDate, DateTime iToDate, int hotelId, string divId)
    {
        StringBuilder ordersTable = new StringBuilder();
        int counter = 0;
        int total_rooms = 0;
        string trDesign = "<tr style='background-color:#dcdcdc;'>";

		ordersTable.Append("<div id='div_" + divId + "' style='display:none;'>");
		
        ordersTable.Append("<table width='100%' dir='rtl' class='trans' border='1'>");
        ordersTable.Append("<tr>");
        //ordersTable.Append("<th>שם מלון</th>");
        ordersTable.Append("<th>מס. שובר</th>");
        ordersTable.Append("<th>מס. תיק</th>");
        ordersTable.Append("<th>מס. הזמנה</th>");
        ordersTable.Append("<th>תאריך</th>");
        ordersTable.Append("<th>תאריך עזיבה</th>");
        ordersTable.Append("<th>מספר לילות</th>");
		ordersTable.Append("<th>מספר חדרים</th>");
        ordersTable.Append("<th>בסיס ארוח</th>");
        ordersTable.Append("<th>הרכב</th>");
        ordersTable.Append("<th>שם אורח</th>");

        ordersTable.Append("<th >הערה לספק</th>");

        ordersTable.Append("</tr>");
 


		//DateTime currentDate = iFromDate.AddDays(i);
		//DateTime toDate =
		DataSet ds2 = DAL_SQL_Helper.GetCurrentAllocationsForReport(iCurrentDate, hotelId);
        int lastVoucherId = 0;
        
        foreach (DataRow row in ds2.Tables[0].Rows)
        {
            //if (iCurrentDate < DateTime.Parse(row["to_date"].ToString()) && iCurrentDate >= DateTime.Parse(row["from_date"].ToString()))
            if (iCurrentDate >= iFromDate && DateTime.Parse(row["to_date"].ToString()) > iFromDate && DateTime.Parse(row["from_date"].ToString()) <= iToDate
                && iCurrentDate < DateTime.Parse(row["to_date"].ToString()) && iCurrentDate >= DateTime.Parse(row["from_date"].ToString()))
            {
                if (lastVoucherId != int.Parse(row["VaucherId"].ToString()))
                {

                    if (counter % 2 == 0)
                    {
                        trDesign = "<tr style='background-color:#eaeaea; text-align:center;font-family:arial;'>";
                    }
                    else
                    {
                        trDesign = "<tr style='background-color:#ffffff;text-align:center;font-family:arial;'>";
                    }
                    counter++;
                    ordersTable.Append(trDesign);
                    //ordersTable.Append("<td style='text-align:center;'>" + row["HotelName"] + "</td>");
                    ordersTable.Append("<td style='width:2%; text-align:center;'>" + row["VaucherId"] + "</td>");
                    ordersTable.Append("<td style='width:5%; text-align:center;'>" + row["docket_id"] + "</td>");
                    ordersTable.Append("<td style='width:5%; text-align:center;'>" + row["order_number"] + "</td>");
                    ordersTable.Append("<td style='width:8%; text-align:center;' dir='ltr' style='color:orange; '>" + DateTime.Parse(row["from_date"].ToString()).ToShortDateString() + "</td>");
                    ordersTable.Append("<td style='width:8%; text-align:center;' dir='ltr'>" + DateTime.Parse(row["to_date"].ToString()).ToShortDateString() + "</td>");
                    ordersTable.Append("<td style='width:1%; text-align:center;'>" + row["DaysNumber"] + "</td>");
                    ordersTable.Append("<td style='width:1%; text-align:center;'>" + row["RoomsSum"].ToString() + "</td>"); 
                    ordersTable.Append("<td style='width:8%; text-align:center;' align='right'>" + row["BaseName"] + "</td>");
                    ordersTable.Append("<td style='text-align:center;' align='right'>" + row["RoomName"] + "</td>");
                    ordersTable.Append("<td style='width:11%; text-align:center;' align='right'>" + row["TravellerFullName"] + "</td>");

                    ordersTable.Append("<td align='right'>" + row["remark"] + "</td>");

                    ordersTable.Append("</tr>");

                    if (row["RoomName"].ToString().Length > 0)
                        total_rooms++;
                }

                lastVoucherId = int.Parse(row["VaucherId"].ToString());
            }

        }
		
		ordersTable.Append("</table>");
		ordersTable.Append("</div>");
		Response.Write(ordersTable.ToString());
		
        return ordersTable.ToString();
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
        const string postFix = "@fax.vayosoft.com";
        string val = hdnFFax.Value;
        bool res = false;
        if (Utils.ValidateString(val, false))
        {           
            res = Utils.SendMail(String.Concat(val, postFix), resultTable.ToString());           
        }
        hdnF2Res.Value = res.ToString();
    }
}
