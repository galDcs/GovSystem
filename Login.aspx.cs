using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["IsLoggedIn"] != null)
        {
            const string falseAnswer = "false";
            string isLoggedIn = Session["IsLoggedIn"].ToString();

            if (isLoggedIn == falseAnswer)
            {
                lbMessage.Text = "אנא התחבר מחדש";
            }
            
            
        }
        Session.Clear();
    }
    protected void btSubmit_Click(object sender, EventArgs e)
    {
        string travellerID = tbTravellerID.Text;
        string docketID = tbDocketID.Text;

        if (DAL_SQL_Helper.CheckCredentials(travellerID, docketID))
        {
            Session["TravellerID"] = travellerID;
            Session["DocketID"] = docketID;
            Response.Redirect("default.aspx");
        }
        else
        {
            lbMessage.Text = "משתמש לא נמצא, אנא נסו שנית";
        }
    }
}