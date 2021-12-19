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
using System.Globalization;


public partial class CurrentAllocationsReport : System.Web.UI.Page
{
    public StringBuilder dropdown = new StringBuilder();
    public static StringBuilder resultTable = new StringBuilder();
	public int index = 0;


    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Utils.CheckSecurity(230)) Response.Redirect("AccessDenied.aspx");

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

    private void loadInitialData()
    {
        //Create dropdown
        //dropdown.Append("<select id=\"Hotels\" name=\"Hotels\">");
        
        DataSet ds1 = DAL_SQL_Helper.GetHotels();
        ds1.Tables[0].Columns.Add("NameAndArea", typeof(string), "name +' - '+description");

        DataRow selectAll = ds1.Tables[0].NewRow();
        selectAll[0] = 0;
        selectAll[1] = "הכל";
        selectAll[2] = "הכל";
        ds1.Tables[0].Rows.InsertAt(selectAll, 0);

        Hotels.DataSource = ds1.Tables[0];
        Hotels.DataBind();
        //foreach (DataRow row in ds1.Tables[0].Rows)
        //{
        //    dropdown.Append("<option value=\"" + row["id"].ToString() + "\">"+ row["name"].ToString() +"</option>");
        //}
        //dropdown.Append("</select>");
    }

    private void loadReportData()
    {
        string hotelId = "0";
		
        try
        {
            if (txtFromDate.Text.Length > 0 && txtToDate.Text.Length > 0)
            {
                lbMessage.Text = "";
                DateTime fromDate = new DateTime();
                DateTime toDate = new DateTime();

                if (DateTime.TryParse(txtFromDate.Text, out fromDate) && DateTime.TryParse(txtToDate.Text, out toDate))
                {
                    hotelId = Hotels.SelectedValue;
                    fromDate = DateTime.Parse(txtFromDate.Text);
                    toDate = DateTime.Parse(txtToDate.Text);
                    if (hotelId != "0")
                    {
                        showSelectedDate(fromDate, toDate, hotelId, Hotels.SelectedItem.Text);
                    }
                    else
                    {
                        foreach (ListItem hotel in Hotels.Items)
                        {
                            if (hotel.Value != "0")
                            {
                                hotelId = hotel.Value;
                                showSelectedDate(fromDate, toDate, hotelId, hotel.Text);
                            }
                        }
                    }
                }
            }
            else
            {
                lbMessage.Text = "אנא בחר/י תאריכים";
            }
        }
        catch (Exception exc)
        {
            Logger.Log(exc.Message);
            string message = "An error occured while trying to get the data.";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + message + "');", true);
        }
    }

    private void showSelectedDate(DateTime iFromDate, DateTime iToDate, string iHotelId, string iHotelName)
    {
        StringBuilder resultTableTemp = new StringBuilder();
        string trDesign = "<tr style='background-color:#dcdcdc;'>";
        int total_rooms = 0;
        int counter = 0;
		
//style='page-break-before: always;'
        resultTableTemp.Append("<table width=\"100%\" border='1' dir=\"rtl\" class=\"trans\" style='page-break-before: always;'>");
		
		resultTableTemp.Append("<tr><td class='hide_element' colspan="+10+">"+Utils.GetHeadelLogoReport()+"</td></tr>");
		
        

        resultTableTemp.Append("<tr>");
        resultTableTemp.Append("<td colspan=\"" + 11 + "\" style=\"border-bottom: solid 1px gray;\">");
        resultTableTemp.Append("<table width=\"100%\" dir=\"rtl\">");
        resultTableTemp.Append("<tr>");
        resultTableTemp.Append("<td style=\"font-size:22px;border: solid 0px black;\"> מימוש לינות יומי - " + iHotelName + "</td>");
        resultTableTemp.Append("</tr>");
        resultTableTemp.Append("</table>");
        resultTableTemp.Append("</td>");
        resultTableTemp.Append("</tr>");

        resultTableTemp.Append("<tr style=\"border: solid 0px black;\">");
        resultTableTemp.Append("<td colspan=\"" + 11 + "\" style=\"border: solid 0px black;\">");
        resultTableTemp.Append("<table width=\"100%\" dir=\"rtl\" style=\"border: solid 0px black;\">");
        resultTableTemp.Append("<tr style=\"border: solid 0px black;\">");
        resultTableTemp.Append("<td style=\"border: solid 0px black;\">מתאריך" + " : " + Utils.GetFormattedDTString(iFromDate) + "</td>");
        resultTableTemp.Append("<td style=\"border: solid 0px black;\">עד תאריך" + " : " + Utils.GetFormattedDTString(iToDate) + "</td>");
        resultTableTemp.Append("<td style=\"border: solid 0px black;\">מלון" + " : " + iHotelName + "</td>");
        resultTableTemp.Append("</tr>");
        resultTableTemp.Append("</table>");
        resultTableTemp.Append("</td>");
        resultTableTemp.Append("</tr>");


        resultTableTemp.Append("<tr>");
        resultTableTemp.Append("<th>תאריך</th>");
		resultTableTemp.Append("<th>מס. תיק</th>");
        resultTableTemp.Append("<th>מס. שובר</th>");
        resultTableTemp.Append("<th>שם אורח</th>");
        resultTableTemp.Append("<th>הרכב</th>");
        resultTableTemp.Append("<th>בסיס ארוח</th>");
        resultTableTemp.Append("<th>מס לילות</th>");
        resultTableTemp.Append("<th>תאריך כניסה</th>");
        resultTableTemp.Append("<th>תאריך עזיבה</th>");
        resultTableTemp.Append("<th>מס. הזמנה</th>");
        resultTableTemp.Append("<th width=\"150px\">הערה לספק</th>");
        resultTableTemp.Append("</tr>");


        int datediff = iToDate.Subtract(iFromDate).Days;

        //added 31 to count also the orders that starts at the end of month and end in the beggining of the requsted month.
        for (int i = 0; i <= datediff + 31; i++)
        {
            DateTime currentDate = iFromDate.AddDays(i - 31);
            //DateTime dbFormattedDate = DateTime.ParseExact(fromDate.To, "dd/MMM/yyyy", CultureInfo.InvariantCulture);

            DataSet ds2 = DAL_SQL_Helper.GetCurrentAllocationsForReport(currentDate, Convert.ToInt32(iHotelId));
            string prevVoucher = String.Empty;

            if (ds2 != null && ds2.Tables.Count > 0 && ds2.Tables[0].Rows != null && ds2.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds2.Tables[0].Rows)
                {
                    if (currentDate >= iFromDate && DateTime.Parse(row["to_date"].ToString()) > iFromDate && DateTime.Parse(row["from_date"].ToString()) <= iToDate)
                    {
                        index++;
						
                        if (counter % 2 == 0)
                        {
                            trDesign = "<tr style='background-color:#eaeaea; text-align:center;font-family:arial;'>";
                        }
                        else
                        {
                            trDesign = "<tr style='background-color:#ffffff;text-align:center;font-family:arial;'>";
                        }

                        //no RoomName -> melave
                        if (row["RoomName"].ToString().Length > 0 && !prevVoucher.Equals(row["VaucherId"].ToString()))
                        {
                            counter++;
                            resultTableTemp.Append(trDesign);
                            resultTableTemp.Append("<td dir='ltr'>" + currentDate.ToShortDateString() + "</td>");
							resultTableTemp.Append("<td>" + "<A HREF='/AGN_SRC/Dockets/Main/main_screen.asp?DocketID=" +row["docket_id"]+ "' TARGET='_blank'" + ">" +row["docket_id"]+ "</A>" + "</td>");
                            resultTableTemp.Append("<td>" + row["VaucherId"] + "</td>");
							
                            resultTableTemp.Append("<td align='right'>" + row["TravellerFullName"] + "</td>");
                            resultTableTemp.Append("<td align='right'>" + row["RoomName"] + "</td>");
                            resultTableTemp.Append("<td align='right'>" + row["BaseName"] + "</td>");
                            resultTableTemp.Append("<td>" + row["DaysNumber"] + "</td>");
                            resultTableTemp.Append("<td dir='ltr'>" + row["from_date"] + "</td>");
                            resultTableTemp.Append("<td dir='ltr'>" + row["to_date"] + "</td>");
                            resultTableTemp.Append("<td>" + row["order_number"] + "</td>");
                            resultTableTemp.Append("<td align='right'>" + row["remark"] + "</td>");
                            resultTableTemp.Append("</tr>");
                            total_rooms++;
                        }

                        if (row["RoomName"].ToString().Length > 0)
                        {
                            prevVoucher = row["VaucherId"].ToString();
                        }
                    }
                }
            }
        }
        resultTableTemp.Append("<tr><td>Total</td><td colspan='11'>" + total_rooms + "</td></tr>");

        resultTableTemp.Append("</table>");
        if (total_rooms > 0)
        {
            resultTable.Append(resultTableTemp);
        }

        //DataSet ds1 = DAL_SQL_Helper.GetBCInvoicesForInvoicesFile(fromDate, toDate);

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
        string toAddress = Utils.GetHotelFaxToAddress(Hotels.SelectedValue);

        bool res = Utils.SendMail(toAddress, resultTable.ToString());
        hdnFFax.Value = res.ToString();
    }
}
