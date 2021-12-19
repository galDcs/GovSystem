using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;

public partial class ManageHolidays : System.Web.UI.Page
{
    public static StringBuilder resultHolidays = new StringBuilder();
    public static int rowIndex = 0;
    protected void Page_Load(object sender, EventArgs e)
    {

        //if (!Utils.CheckSecurity(243)) Response.Redirect("AccessDenied.aspx");

        if (Page.IsPostBack)
        {
            if (Request.Form["hdAction"] == "actionSave")
            {
                saveHolidays();
            }
        }

        initialLoad();
    }

    private void saveHolidays()
    {
        int currentRowIndex = Convert.ToInt32(Request.Form["hdCurrentRowIndex"]);
        for (int i = 1; i <= currentRowIndex; i++)
        {
            if (Request.Form["chbDelete" + i] == "on")
            {
                if (Request.Form["holidayPK" + i] != "0")
                {
                    DAL_SQL_Helper.DeleteHolidays(Convert.ToInt32(Request.Form["holidayPK" + i]));
                }
            }
            else
            {
                try
                {
                    DateTime date = Convert.ToDateTime(Request.Form["datepicker" + i]);
                    DAL_SQL_Helper.UpsertHolidays(Convert.ToInt32(Request.Form["holidayPK" + i]), date, Request.Form["description" + i]);
                }
                catch (Exception ex)
                {
                    lblMessage.Visible = true;
                    lblMessage.Text = "שגיעה בעדכון נתונים";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    return;
                }
            }
        }
        lblMessage.Visible = true;
        lblMessage.Text = "נתונים נישמרו בהצלחה";
        lblMessage.ForeColor = System.Drawing.Color.Green;
    }

    public void initialLoad()
    {
        resultHolidays = new StringBuilder();
        rowIndex = 0;
        DataSet ds1 = DAL_SQL_Helper.GetHolidays();
        resultHolidays.Append("<table id='mainTable' class='MainTable' dir='rtl' style='width:80%; align:center;'>");
        resultHolidays.Append("<tr><th style='width:10%'>תאריך</th><th style='width:40%'>שם</th><th style='width:15%'></th></tr>");
        foreach(DataRow row in ds1.Tables[0].Rows)
        {
            rowIndex++;
            //DateTime currentDate = Convert.ToDateTime(row["HolidayDate"]).ToString("dd-mm-yy");
            //resultHolidays.Append("<span style='display:block;'><input id='holidayPK" + rowIndex.ToString() + "' name='holidayPK" + rowIndex.ToString() + "' type='hidden' value='" + row["id"] + "'><input type='text' id='datepicker" + rowIndex.ToString() + "' name='datepicker" + rowIndex.ToString() + "' class='datepicker' value='" + Convert.ToDateTime(row["HolidayDate"]).ToString("dd/MM/yy") + "'/><input type='text' id='description" + rowIndex.ToString() + "' name='description" + rowIndex.ToString() + "' value='" + row["HolidayDescription"] + "'><input type='checkbox' id='chbDelete" + rowIndex.ToString() + "' name='chbDelete" + rowIndex.ToString() + "'/>Delete </span>");
            resultHolidays.Append("<tr>");
            resultHolidays.Append("<input id='holidayPK" + rowIndex.ToString() + "' name='holidayPK" + rowIndex.ToString() + "' type='hidden' value='" + row["id"] + "' />");
            resultHolidays.Append("<td><input type='text' id='datepicker" + rowIndex.ToString() + "' name='datepicker" + rowIndex.ToString() + "' class='datepicker inputWide' value='" + Convert.ToDateTime(row["HolidayDate"]).ToString("dd/MM/yy") + "' /></td>");
            resultHolidays.Append("<td><input type='text' id='description" + rowIndex.ToString() + "' name='description" + rowIndex.ToString() + "' class='inputWide' value='" + row["HolidayDescription"] + "' /> </td>");
            resultHolidays.Append("<td><input type='checkbox' id='chbDelete" + rowIndex.ToString() + "' name='chbDelete" + rowIndex.ToString() + "' />סמן למחיקה</td>");
            resultHolidays.Append("</tr>");
        }
        resultHolidays.Append("</table>");
    }
}