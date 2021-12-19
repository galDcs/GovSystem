using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Error : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string msg = string.Empty;


        if (Session["IsLoggedIn"] != null)
        {
            msg = Session["IsLoggedIn"].ToString().Trim();
			if (msg =="Access token אינו תקין או פג תוקף")
			{
				msg = "<h2 style='direction:ltr;text-decoration: underline;'>" + msg + "</h2>"
					  +"<ul style='direction: rtl;text-align: right;font-weight: normal;'>"
					  +"<li>הזכאי קיים בקובץ זכאים האחרון שהועלה אך לא קיים במשרד הביטחון</li>"
					  +"<li>שימוש ארוך בדף המבוקש</li>"
					  +"</ul>";
			}
			Session["IsLoggedIn"] = null;
        }
		else if (Session["ClientMessage"] != null)
        {
            msg = Session["ClientMessage"].ToString().Trim();
			
			Session["ClientMessage"] = null;
        }
        else
        {
            msg = "זמן ההתחברות תם אנא כנס/י מחדש";
        }

        lbMsgError.Text = msg;
    }
}