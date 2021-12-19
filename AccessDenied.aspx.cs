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

public partial class AccessDenied : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        ShowMessage("Access Denied",MessageType.Error);
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
}
