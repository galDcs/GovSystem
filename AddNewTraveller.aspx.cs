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
using System.Globalization;

public partial class AddNewTraveller : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Utils.CheckSecurity(227)) Response.Redirect("AccessDenied.aspx");

        messageContainer.Visible = false;
        divResult.Visible = false;
    }

    protected void AddNewOnClick(object sender, EventArgs e)
    {
        //DataTable dt = (DataTable)rpt.DataSource;
        //dt.Rows.Clear();
        //rpt.DataSource = dt;
        //rpt.DataBind();
    }
    protected void SeachOnClick(object sender, EventArgs e)
    {
        lblError.Text = "";
        string travellerId = txtTravellerId.Text;
        string docketId = txtDocketId.Text;
        DataSet ds;
        if (((Button)sender).CommandName == "addnew")
        {
            ds = DAL_SQL_Helper.GetTravellerDetailsDS(string.Empty, string.Empty);
            if (ds.Tables[0].Rows.Count > 0) ds.Tables[0].Rows.Clear();
            object[] items = new object[ds.Tables[0].Columns.Count];
            for (int i = 0; i < items.Length; i++)
            {
                items[0] = null;
            }
            ds.Tables[0].Rows.Add(items);
        }
        else
        {

            if (travellerId.Length > 0) // search by traveller id
            {
                ds = DAL_SQL_Helper.GetTravellerDetailsDS(travellerId, string.Empty);
            }
            else if (docketId.Length > 0)
            {
                ds = DAL_SQL_Helper.GetTravellerDetailsDS(string.Empty, docketId);
            }
            else
            {
                ShowMessage("נא להזין פרטי לחיפוש.", MessageType.Error);
                return;
            }
        }

        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        {
            rpt.DataSource = ds;
            rpt.DataBind();
            divResult.Visible = true;
        }
        else
        {
            ShowMessage("הזכאי הוסר מקובץ הזכאים של משרד הביטחון", MessageType.Info);
            divResult.Visible = false;
        }
    }

    protected void SaveBtnClick(object sender, EventArgs e)
    {
        lblError.Text = "";


        foreach (RepeaterItem rItem in rpt.Items)
        {
            //((TextBox)rItem.FindControl("DocketId")).Text
            try
            {
                int status = 1;
                bool IsActive = (((CheckBox)rItem.FindControl("chIsActive")).Checked ? true : false);
                string docketId = ((TextBox)rItem.FindControl("tBoxDocketID")).Text;
                string travellerId = ((TextBox)rItem.FindControl("tBoxTravellerID")).Text;
                string makatNumber = ((TextBox)rItem.FindControl("tBoxItemSKU")).Text;

                docketId = Utils.AddAbsentSignsToTheBeginningOfTheString(docketId, 9, "0");
                travellerId = Utils.AddAbsentSignsToTheBeginningOfTheString(travellerId, 9, "0");
                makatNumber = Utils.AddAbsentSignsToTheBeginningOfTheString(makatNumber, 6, "0");

                if (((TextBox)rItem.FindControl("tBoxStatus")).Text.Length <= 0)
                {
                    status = 1;
                }
                status = int.Parse(((TextBox)rItem.FindControl("tBoxStatus")).Text);

                DAL_SQL_Helper.UpsertTravellerUpdate(
                        docketId, // DocketId
                        travellerId, // TravellerID
                        ((TextBox)rItem.FindControl("tBoxFirstName")).Text, // FirstName
                        ((TextBox)rItem.FindControl("tBoxSecondName")).Text, // SecondName
                        ((TextBox)rItem.FindControl("tBoxAddress")).Text, // Address
                        ((TextBox)rItem.FindControl("tBoxCityCode")).Text, // CityCode
                        ((TextBox)rItem.FindControl("tBoxCity")).Text, // City
                        ((TextBox)rItem.FindControl("tBoxZipCode")).Text, // ZipCode
                        makatNumber, // ItemSKU
                        ((TextBox)rItem.FindControl("tBoxItemDesc")).Text,  // ItemDescription ,
                        DateTime.Parse(((TextBox)rItem.FindControl("tBoxStartDate")).Text), // StartDate
                        DateTime.Parse(((TextBox)rItem.FindControl("tBoxEndDate")).Text), // EndDate
                        int.Parse(((TextBox)rItem.FindControl("tBoxDaysNum")).Text), // DaysNum
                        int.Parse(((TextBox)rItem.FindControl("tBoxEscortNum")).Text), // EscortNum ,
                        int.Parse(((TextBox)rItem.FindControl("tBoxDepartment")).Text), // Department
                        int.Parse(((TextBox)rItem.FindControl("tBoxLevel")).Text), // Level
                        status, // Status
                        int.Parse(((TextBox)rItem.FindControl("tBoxUsageBalance")).Text),//UsageBalance
                        int.Parse(((TextBox)rItem.FindControl("tBoxJerusalemUsageBalance")).Text),//JerusalemUsageBalance
                        ((TextBox)rItem.FindControl("txbPhonePrefix1")).Text,  // txbPhonePrefix1
                        ((TextBox)rItem.FindControl("txbPhoneNumber1")).Text,  // txbPhoneNumber1
                        ((TextBox)rItem.FindControl("txbPhonePrefix2")).Text,  // txbPhonePrefix2
                        ((TextBox)rItem.FindControl("txbPhoneNumber2")).Text,  // txbPhoneNumber2
                        ((TextBox)rItem.FindControl("txbPhonePrefix3")).Text,  // txbPhonePrefix3
                        ((TextBox)rItem.FindControl("txbPhoneNumber3")).Text, // txbPhoneNumber3                       
                        ((TextBox)rItem.FindControl("txbOfficeComments")).Text, //txbOfficeComments
                        DateTime.Parse(((TextBox)rItem.FindControl("tReleaseDate")).Text), //tReleaseDate
                        IsActive,
                        int.Parse(((TextBox)rItem.FindControl("tMakat40")).Text) //tMakat40
                );
                lblError.ForeColor = System.Drawing.Color.Green;
                lblError.Text = "פעולה בוצע בהצלחה.";
            }
            catch (Exception ex)
            {
                lblError.ForeColor = System.Drawing.Color.Red;
                lblError.Text = ex.Message + " --- " + ex.Source + " --- " + ex.StackTrace;
            }
        }
    }

    private void ShowMessage(string strMessage, MessageType type)
    {
        switch (type)
        {
            case MessageType.Error:
                Message.Text = strMessage;
                messageContainer.Visible = true;
                messageContainer.Style.Add("color", "red");
                break;
            case MessageType.Info:
                Message.Text = strMessage;
                messageContainer.Visible = true;
                messageContainer.Style.Add("color", "blue");
                break;
            case MessageType.Warning:
                Message.Text = strMessage;
                messageContainer.Visible = true;
                messageContainer.Style.Add("color", "yellow");
                break;
        }
    }

    protected string FormatDate(object strDate)
    {
        try
        {
            DateTime dt = ((DateTime)strDate);
            return String.Format("{0:dd'/'MM'/'yyyy}", dt);
            //return dt.ToString("dd/MM/yyyy");
            //if (strDate typeof DateTime)
            //{
                //CultureInfo ci = new CultureInfo(CultureInfo.CurrentCulture.LCID);
                //ci.Calendar.TwoDigitYearMax = 2099;
                //DateTime date = DateTime.ParseExact(strDate, "dd-MMM-yy", ci);
                //return date.ToString("dd/MMM/yyyy");
            //}
            return "";
        }
        catch (Exception e)
        {
            return "";
        }
    }
    protected bool getStatus(object status)
    {
        bool ret = true;
        if (status is System.Boolean)
        {
            ret = (bool)status;
        }
        return ret;
    }

}
